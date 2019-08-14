import React from "react";
import ReactTable from "react-table";
import withFixedColumns from "react-table-hoc-fixed-columns";
import DatePicker from "react-datepicker";
import moment from "moment";
const ReactTableFixedColumns = withFixedColumns(ReactTable);
class JobAbandonmentCount extends React.Component {
  columns = [];
  constructor(props) {
    super(props);
    this.state = {
      show: false,
      date: {
        ge: null,
        le: null
      }
    };
    this.validateDates = this.validateDates.bind(this);
    this.handleClick = this.handleClick.bind(this);
  }

  toLocalDate(dateTime) {
    return moment(dateTime).format("MM/DD/YYYY");
  }

  handleStartDateChange(date) {
    this.setState({
      date: {
        ge: date,
        le: this.state.date.le
      }
    });
  }

  handleEndDateChange(date) {
    this.setState({
      date: {
        ge: this.state.date.ge,
        le: date
      }
    });
  }

  validateDates() {
    const { date } = this.state;
    if (date.ge == null || date.le == null) {
      ToastService.error("Enter a valid From and To dates");
      return;
    }
    var startDate = this.toLocalDate(date.ge);
    var endDate = this.toLocalDate(date.le);
    if (startDate <= endDate) {
      this.fetchData(startDate, endDate);
    } else {
      ToastService.error("End date must be after Start date");
    }
  }

  fetchData(startDate, endDate) {
    this.setState({ loading: true }, () =>
      CareerCircleAPI.getJobAbandonmentCount(startDate, endDate)
        .then(res => {
          this.setState({ show: true, data: res.data });
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
    this.validateDates();
  }

  render() {
    const { data, date } = this.state;
    return (
      <div className="react-report-table">
        <div className="advanced-filters-wrapper p-2">
          <div className="form-inline">
            <div className="form-group mr-1">
              <div className="d-block">
                <DatePicker
                  placeholderText="From Date"
                  selected={date.ge}
                  showYearDropdown
                  selectsStart
                  startDate={date.ge}
                  endDate={date.le}
                  isClearable={true}
                  onChange={date => this.handleStartDateChange(date)}
                  maxDate={new Date()}
                />
              </div>
            </div>
            -
            <div className="form-group ml-1">
              <div className="d-block">
                <DatePicker
                  placeholderText="To Date"
                  selected={date.le}
                  showYearDropdown
                  selectsEnd
                  startDate={date.ge}
                  endDate={date.le}
                  isClearable={true}
                  onChange={date => this.handleEndDateChange(date)}
                  maxDate={new Date()}
                />
              </div>
            </div>
            <div className="form-group ml-3">
              <div className="d-block">
                <i onClick={this.handleClick} className="fas fa-search" />
              </div>
            </div>
          </div>
        </div>
        {this.state.show && (
          <ReactTableFixedColumns
            data={data}
            columns={this.state.columns}
            loading={this.state.loading}
            style={{
              height: "300px"
            }}
            columns={[
              {
                Header: "Date",
                accessor: "ActionCreateDate",
                Cell: data => (
                  <React.Fragment>
                    {this.toLocalDate(data.original.actionCreateDate)}
                  </React.Fragment>
                ),
                Footer: (
                  <span style={{ float: "right" }}>
                    <strong>Total:</strong>
                  </span>
                )
              },
              {
                Header: "Count",
                accessor: "count",
                headerClassName: "text-right mr-2",
                className: "text-right mr-2",
                Footer: (
                  <strong>
                    {data && data.reduce((sum, { count }) => sum + count, 0)}
                  </strong>
                )
              }
            ]}
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

export default JobAbandonmentCount;
