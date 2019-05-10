import React from 'react';
import ReactTable from 'react-table';
import withFixedColumns from 'react-table-hoc-fixed-columns';
import DatePicker from 'react-datepicker';

const ReactTableFixedColumns = withFixedColumns(ReactTable);

class PCATable extends React.Component {
    columns = [];
    constructor(props) {
        super(props);
        this.buildColumns(props);
        this.state= {
            data: props.data.report,
            loading: false,
            date: {
                ge: null,
                le: null
            }
        };
    }

    buildColumns(props) {
        this.columns = props.data.actionKey.map(x => {
            return {
                Header: x.name,
                id: "col-actionId" + x.actionId,
                width: 175,
                accessor: (data) => data.stats[x.actionId] || 0
            }
        });

        this.columns.unshift({
            Header: "Partner",
            fixed: "left",
            minWidth: 200,
            accessor: "partnerName"
        });
    }

    handleStartDateChange(date) {
        this.setState({ date: { ge: date, le: this.state.date.le }}, () => this.reloadData());
    }

    handleEndDateChange(date) {
        this.setState({ date: { ge: this.state.date.ge, le: date }}, () => this.reloadData());
    }

    getQuery() {
        const query = { filter: {} };
        if(this.state.date.ge || this.state.date.le)
            query.filter.createDate = {};
        if(this.state.date.ge)
            query.filter.createDate.ge = this.state.date.ge;
        if(this.state.date.le)
            query.filter.createDate.le = this.state.date.le;
        console.log(query);
        return query;
    }

    reloadData() {
        this.setState({loading: true}, () =>
            CareerCircleAPI.getPartnerContactActions(this.getQuery())
                .then((res) => {
                    this.setState({data: res.data.report});
                })
                .catch(() => ToastService.error('An error occured while loading the report.'))
                .finally(() => {
                    this.setState({loading: false});
                })
        );
    }

    render() {
        return (
            <div className="pca-table">
                <div className="advanced-filters-wrapper p-2">
                    <div className="form-inline">
                    <div className="form-group mr-2">
                        <label className="mr-2">Start Date:</label>
                        <div className="d-block">
                            <DatePicker
                                selected={this.state.date.ge}
                                onChange={(date) => this.handleStartDateChange(date)}
                                showYearDropdown
                                isClearable={true}
                                maxDate={new Date()}
                            />
                        </div>
                    </div>
                    <div className="form-group">
                        <label className="mr-2">End Date:</label>
                        <div className="d-block">
                            <DatePicker
                                selected={this.state.date.le}
                                onChange={(date) => this.handleEndDateChange(date)}
                                showYearDropdown
                                isClearable={true}
                                maxDate={new Date()}
                            />
                        </div>
                    </div>
                    </div>
                </div>
                <ReactTableFixedColumns
                    data={this.state.data}
                    columns={this.columns}
                    loading={this.state.loading}
                    showPagination={false}
                    className="-striped -highlight"
                    minRows={4}
                    noDataText="No Partner Contact Actions"
                />
            </div>
        );
    }
}

export default PCATable;