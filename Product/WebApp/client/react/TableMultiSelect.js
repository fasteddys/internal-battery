import React from 'react';
import ReactTable from 'react-table';

class TableMultiSelect extends React.Component {

    constructor(props) {
        super(props);
        this.state = {
            totalRecords: 0,
            contacts: [],
            selected: {},
            selectAll: 0,
            selectAllFlag: false,
            pageIndex: 1,
            pageSize: 3,
            pages: 0
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
                Header: 'CreateDate',
                accessor: 'createDate'
            }
        ];
        this.fetchData = this.fetchData.bind(this);
    }
    
    componentDidMount() {
        this.WAIT_INTERVAL = 500;
        this.timer = null;
    }

    getRecordsSelected() {
        if(this.state.selectAllFlag)
            return this.state.totalRecords - Object.keys(this.state.selected).length;

        return Object.keys(this.state.selected).length;
    }

    isChecked(contactId) {
        const isSelected = this.state.selected[contactId] !== undefined;
        if(this.state.selectAll !== 0 && this.state.selectAllFlag)
            return !isSelected;

        return isSelected;
    }

    fetchData(state, instance) {
        clearTimeout(this.timer);
        this.timer = setTimeout(() => { this.triggerChange(state) }, this.WAIT_INTERVAL);
    }

    triggerChange(state) {
        CareerCircleAPI.getContacts(state.page + 1, state.pageSize, state.sorted, state.filtered).then((res) => {
            this.setState({ contacts: res.data.data, pages: res.data.pages, totalRecords: res.data.totalRecords });
        });
    }

    toggleRow(contactId) {
        const newSelected = Object.assign({}, this.state.selected);
        newSelected[contactId] = !this.state.selected[contactId];

        if(!newSelected[contactId])
            delete newSelected[contactId];

        this.setState({selected: newSelected, selectAll: 2});
    }

    toggleSelectAll() {
        this.setState({selectAll: this.state.selectAll === 0 ? 1 : 0, selectAllFlag: this.state.selectAll === 0, selected: {}});
    }

    render() {
        return (
            <div>
                <div>{this.getRecordsSelected()}</div>
                <ReactTable manual filterable data={this.state.contacts} columns={this.columns} onFetchData={this.fetchData} pages={this.state.pages}/>
            </div>
        );
    }
}

export default TableMultiSelect;