import React from 'react';
import ReactTable from 'react-table';
import DatePicker from 'react-datepicker';

class ContactSelect extends React.Component {

    constructor(props) {
        super(props);
        this.state = {
            totalRecords: 0,
            contacts: [],
            startDate: null,
            endDate: null,
            page: 0,
            pages: 0,
            pageSize: 20,
            sorted: [],
            filtered: []
        };

        this.columns = [
            {
                Header: 'Name',
                id: 'name',
                accessor: d => d.firstName + " " + d.lastName
            },
            {
                Header: 'Email',
                accessor: 'email'
            },
            {
                Header: "Create Date",
                accessor: d => moment(new Date(d.createDate)).format('L'),
                id: "createDate",
                filterable: false
            }
        ];
    }
    
    componentDidMount() {
        this.WAIT_INTERVAL = 500;
        this.timer = null;
    }

    fetchData = (state, instance) => {
        clearTimeout(this.timer);
        this.timer = setTimeout(() => { this.triggerChange(state, this.state.startDate, this.state.endDate) }, this.WAIT_INTERVAL);
    }

    handleStartDateChange = (date) => {
        this.setState({startDate: date, page: 0}, () => {
            this.triggerChange(this.state);
        });
    }

    handleEndDateChange = (date) => {
        this.setState({endDate: date, page: 0}, () => {
            this.triggerChange(this.state);
        });
    }

    onSave = () => {
        const { startDate, endDate, filtered } = this.state;
        console.log({ startDate, endDate, filtered });        
    }

    triggerChange(state) {
        CareerCircleAPI.getContacts(state.page + 1, state.pageSize, state.sorted, state.filtered, this.state.startDate, this.state.endDate).then((res) => {
            this.setState({ 
                page: state.page,
                contacts: res.data.data,
                pages: res.data.pages,
                totalRecords: res.data.totalRecords
            });
        });
    }

    render() {
        return (
            <div className="row no-gutters">
                <div className="col-md">
                    <div className="row no-gutters">
                        <div className="col-md">
                            Total Selected Contacts: <span>{this.state.totalRecords}</span>
                        </div>
                    </div>
                    <div className="row no-gutters mb-3">
                        <div className="col-md-3">
                            <div>
                                <div>Start Date:</div>
                                <DatePicker
                                    selected={this.state.startDate}
                                    onChange={this.handleStartDateChange}
                                    showYearDropdown
                                    isClearable={true}
                                    maxDate={new Date()}
                                />
                            </div>
                        </div>
                        <div className="col-md-3">
                            <div>End Date:</div>
                            <DatePicker
                                selected={this.state.endDate}
                                selectsEnd
                                startDate={this.state.startDate}
                                endDate={this.state.endDate}
                                isClearable={true}
                                onChange={this.handleEndDateChange}
                            />
                        </div>
                    </div>
                    <div className="row no-gutters">
                        <div className="col-md">
                            <ReactTable 
                                manual 
                                filterable 
                                data={this.state.contacts}
                                columns={this.columns}
                                onFetchData={this.fetchData}
                                page={this.state.page}
                                pages={this.state.pages}
                                pageSize={this.state.pageSize}
                                filtered={this.state.filtered}
                                sorted={this.state.sorted}
                                onPageSizeChange={pageSize => this.setState({pageSize})}
                                onPageChange={page => this.setState({page})}
                                onFilteredChange={filtered =>   this.setState({ filtered }) }
                                onSortedChange={sorted => this.setState({ sorted })}
                            />
                        </div>
                    </div>
                    <div className="row no-gutters">
                        <div className="col-md">
                            <button className="float-right btn btn-primary" onClick={this.onSave}>Save</button>
                        </div>
                    </div>
                </div>
            </div>
        );
    }
}

export default ContactSelect;