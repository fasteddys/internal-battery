import React from 'react';
import ReactTable from 'react-table';
import withFixedColumns from 'react-table-hoc-fixed-columns';
import Tooltip from '@material-ui/core/Tooltip';

const ReactTableFixedColumns = withFixedColumns(ReactTable);

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
                {
                    Header: "CloudTalentIndexInfo",
                    accessor: "cloudTalentIndexInfo",
                    show: false
                },
                {
                    Header: "CloudTalentIndexInfo",
            
                    Cell: ({ row }) => (
                        <Tooltip title={row.cloudTalentIndexInfo}>
                        <p>{row.cloudTalentIndexInfo}</p>
                         </Tooltip>       
                    )
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
                    showPagination={true}
                    className="-striped -highlight"
                    minRows={4}
                    noDataText="No Records"
                    showPagination={true}
                    showPaginationTop= {false}
                    showPaginationBottom= {true}
                    showPageSizeOptions= {true}
                    pageSizeOptions= {[5, 10, 20, 25, 50, 100]}
                    defaultPageSize= {20}
                />
            </div>
        );
    }
}

export default FailedSubscribers;