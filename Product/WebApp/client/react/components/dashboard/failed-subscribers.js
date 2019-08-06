import React from 'react';
import ReactTable from 'react-table';
import withFixedColumns from 'react-table-hoc-fixed-columns';
const ReactTableFixedColumns = withFixedColumns(ReactTable);

const divStyle = {
    width: '150px'
    
  };

class FailedSubscribers extends React.Component {
    columns = [];
    constructor(props) {
        super(props);
        this.state= {
            data: props.data,
            loading: false,
            date: {
                ge: null,
                le: null
            },
            columns: [
                {
                    Header: "First Name",
                    accessor: "firstName"
                },
                {
                    Header: "Last Name",
                    accessor: "lastName",
                },
                {
                    Header: "Email",
                    accessor:"email"
                },
                {
                    Header: "Modify Date",
                    accessor: "modified",
                },
            ]
        };
    }


    render() {
        return (
            <div className="react-report-table">
                <ReactTableFixedColumns
                    data={this.state.data}
                    columns={this.state.columns}
                    loading={this.state.loading}
                    showPagination={false}
                    className="-striped -highlight"
                    minRows={4}
                    noDataText="No Records"
                />
            </div>
        );
    }
}

export default FailedSubscribers;