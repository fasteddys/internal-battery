import React from 'react';
import ReactTable from 'react-table';
import DatePicker from 'react-datepicker';

class TableMultiSelect extends React.Component {

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
        this.fetchData = this.fetchData.bind(this);
        this.handleStartDateChange = this.handleStartDateChange.bind(this);
        this.handleEndDateChange = this.handleEndDateChange.bind(this);
    }
    
    componentDidMount() {
        this.WAIT_INTERVAL = 500;
        this.timer = null;
    }

    fetchData(state, instance) {
        clearTimeout(this.timer);
        this.timer = setTimeout(() => { this.triggerChange(state, this.state.startDate, this.state.endDate) }, this.WAIT_INTERVAL);
    }

    handleStartDateChange(date) {
        this.setState({startDate: date, page: 0}, () => {
            this.triggerChange(this.state);
        });
    }

    handleEndDateChange(date) {
        this.setState({endDate: date, page: 0}, () => {
            this.triggerChange(this.state);
        });
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
            <div>
                <div>{this.state.totalRecords}</div>
                <DatePicker
                    selected={this.state.startDate}
                    onChange={this.handleStartDateChange}
                    showYearDropdown
                    maxDate={new Date()}
                />
                <DatePicker
                    selected={this.state.endDate}
                    selectsEnd
                    startDate={this.state.startDate}
                    endDate={this.state.endDate}
                    onChange={this.handleEndDateChange}
                />
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
                    onPageSize={pageSize => this.setState({pageSize})}
                    onPageChange={page => this.setState({page})}
                    onFilteredChange={filtered =>   this.setState({ filtered }) }
                    onSortedChange={sorted => this.setState({ sorted })}
                />
            </div>
        );
    }
}

export default TableMultiSelect;