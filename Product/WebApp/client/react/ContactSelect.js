import React from 'react';
import ReactTable from 'react-table';
import DatePicker from 'react-datepicker';
import Select from './Select';

class ContactSelect extends React.Component {

    constructor(props) {
        super(props);
        this.state = {
            totalRecords: 0,
            contacts: [],
            startDate: null,
            endDate: null,
            partner: null,
            page: 0,
            pages: 0,
            pageSize: 20,
            sorted: [],
            filtered: [],
            partners: []
        };

        this.columns = [
            {
                Header: 'Name',
                id: 'name',
                filterable: false,
                sortable: false,
                accessor: d => {
                    if(d.partnerContacts.length == 0)
                        return 'N/A';
                    
                    const metaData = d.partnerContacts[d.partnerContacts.length - 1].metadata;
                    return metaData.FirstName + " " + metaData.LastName;
                }
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
            },
            {
                Header: "Partners",
                id: "partners",
                sortable: false,
                filterable: false,
                accessor: d => d.partnerContacts,
                Cell: props => {
                    const partners = props
                        .value
                        .map((pc) => <span className='badge badge-secondary'>{ this.partnerMap[pc.partnerId].name }</span>);
                    return <div>{ partners }</div>
                }
            }
        ];
    }

    mapPartnerToOption = (option) => { return { value: option.partnerId, label: option.name }};
    
    componentDidMount() {
        this.WAIT_INTERVAL = 500;
        this.timer = null;
        this.partnerMap = {};

        CareerCircleAPI.getPartners().then((res) => {
            this.partnerMap = res.data.reduce((map, obj) => {
                map[obj.partnerId] = obj;
                return map
            }, {});
            this.setState({ partners: res.data });
        });

    }

    handlePartnerChange = (option) => {
        this.setState({ partner: option, page: 0 }, () => {
            this.triggerChange(this.state);
        });
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
        CareerCircleAPI.getContacts(state.page + 1, state.pageSize, state.sorted, state.filtered, this.state.startDate, this.state.endDate, this.state.partner).then((res) => {
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
                            <div className="form-group">
                                <label>Start Date:</label>
                                <DatePicker
                                    className="form-control"
                                    selected={this.state.startDate}
                                    onChange={this.handleStartDateChange}
                                    showYearDropdown
                                    isClearable={true}
                                    maxDate={new Date()}
                                />
                            </div>
                        </div>
                        <div className="col-md-3">
                            <div className="form-group">
                                <label>End Date:</label>
                                <DatePicker
                                    className="form-control"
                                    selected={this.state.endDate}
                                    selectsEnd
                                    startDate={this.state.startDate}
                                    endDate={this.state.endDate}
                                    isClearable={true}
                                    onChange={this.handleEndDateChange}
                                />
                            </div>
                        </div>
                        <div className="col-md-3">
                            <div class="form-group">
                                <label>
                                    Partners:
                                </label>
                                <Select 
                                    options={this.state.partners}
                                    accessor={(option) => { return {label: option.name, value: option.partnerId };}}
                                    onChange={this.handlePartnerChange}
                                    value={this.state.partner}
                                />
                            </div>
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