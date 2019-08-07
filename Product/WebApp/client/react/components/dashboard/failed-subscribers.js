import React from 'react';
import ReactTable from 'react-table';
import withFixedColumns from 'react-table-hoc-fixed-columns';
import Tooltip from '@material-ui/core/Tooltip';
import { withStyles } from '@material-ui/core/styles';
import moment from 'moment';


const HtmlTooltip = withStyles(theme => ({
    tooltip: {
      backgroundColor: '#03264A',
      color: 'white',
      maxWidth: 220,
      fontSize: theme.typography.pxToRem(16),
      border: '1px solid #dadde9',
    },
  }))(Tooltip);
const ReactTableFixedColumns = withFixedColumns(ReactTable);
class FailedSubscribers extends React.Component {
    columns = [];
    constructor(props) {
        super(props);
        this.toLocalDate = this.toLocalDate.bind(this);
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
                    Header: "Create Date",
                    accessor: "createDate",
                    Cell: date => <React.Fragment>{this.toLocalDate(date.value)}</React.Fragment>

                },
                {
                    Header: "CloudTalentIndexInfo",
                    accessor: "cloudTalentIndexInfo",
                    show: false
                },
                {
                    Header: "CloudTalentIndexInfo",
                    accessor: "cloudTalentIndexInfo",
                    Cell: ({ row }) => (
                        <HtmlTooltip  title={row.cloudTalentIndexInfo}>
                        <p>{row.cloudTalentIndexInfo}</p>
                         </HtmlTooltip>       
                    )
                },

            ]
        };

    }

    toLocalDate(dateTime) {
        return moment(dateTime).local().format("MM-DD-YYYY hh:mm a");
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