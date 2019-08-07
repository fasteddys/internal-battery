import React from "react";
import ReactTable from "react-table";
import withFixedColumns from "react-table-hoc-fixed-columns";
import DatePicker from "react-datepicker";

const ReactTableFixedColumns = withFixedColumns(ReactTable);

class JobAppReportTable extends React.Component {
  columns = [];
  constructor(props) {
    super(props);
    this.state = {
      data: null,
      show: props.show,
      loading: false,
      date: {
        ge: null,
        le: null
      }
    };
    this.handleClick = this.handleClick.bind(this);
  }

  buildColumns(props) {
    this.columns = [
      {
        Header: "Company",
        accessor: "companyName"
      },
      {
        Header: "Application Total",
        accessor: "applicationCount",
        headerClassName: "text-right mr-2",
        className: "text-right mr-2"
      }
    ];
  }

  handleStartDateChange(date) {
    this.setState({ date: { ge: date, le: this.state.date.le } }, () =>
      this.reloadData()
    );
  }

  handleEndDateChange(date) {
    this.setState({ date: { ge: this.state.date.ge, le: date } }, () =>
      this.reloadData()
    );
  }

  getQuery() {
    const query = { filter: {} };
    if (this.state.date.ge || this.state.date.le) query.filter.createDate = {};
    if (this.state.date.ge) query.filter.createDate.ge = this.state.date.ge;
    if (this.state.date.le) query.filter.createDate.le = this.state.date.le;
    return query;
  }

  reloadData() {
    this.setState({ loading: true }, () =>
      CareerCircleAPI.getJobAppReport(this.getQuery())
        .then(res => {
          this.setState({ data: res.data });
        })
        .catch(() =>
          ToastService.error("An error occured while loading the report.")
        )
        .finally(() => {
          this.setState({ loading: false });
        })
    );
  }

  handleClick() {
    fetch("/admin/job-application-count")
      .then(res => res.json())
      .then(
        result => {
          this.buildColumns(result);
          this.setState({
            isLoaded: true,
            data: result,
            show: true
          });
        },
        error => {
          this.setState({
            isLoaded: true,
            error
          });
        }
      );
  }

  render() {
    return (
      <div className="react-report-table">
        <div className="advanced-filters-wrapper p-2">
          {!this.state.show &&
          <button className="btn btn-primary ml-0 m-2 " onClick={this.handleClick}>
            Get Data
          </button>}
          {this.state.show && (
            <div className="form-inline">
              <div className="form-group mr-1">
                <div className="d-block">
                  <DatePicker
                    placeholderText="Start Date"
                    selected={this.state.date.ge}
                    onChange={date => this.handleStartDateChange(date)}
                    showYearDropdown
                    selectsStart
                    startDate={this.state.date.ge}
                    endDate={this.state.date.le}
                    isClearable={true}
                    maxDate={new Date()}
                  />
                </div>
              </div>
              -
              <div className="form-group ml-1">
                <div className="d-block">
                  <DatePicker
                    placeholderText="End Date"
                    selected={this.state.date.le}
                    onChange={date => this.handleEndDateChange(date)}
                    showYearDropdown
                    selectsEnd
                    startDate={this.state.date.ge}
                    endDate={this.state.date.le}
                    isClearable={true}
                    maxDate={new Date()}
                  />
                </div>
              </div>
            </div>
          )}
        </div>
        {this.state.show && (
          <ReactTableFixedColumns
            data={this.state.data}
            columns={this.columns}
            loading={this.state.loading}
            showPagination={false}
            className="-striped -highlight"
            minRows={4}
            noDataText="No Records"
          />
        )}
      </div>
    );
  }
}

export default JobAppReportTable;
