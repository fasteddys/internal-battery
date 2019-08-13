import React from "react";
import ReactTable from "react-table";
import withFixedColumns from "react-table-hoc-fixed-columns";
import DatePicker from "react-datepicker";
import moment from "moment";
import Validate from '../Validate';

const ReactTableFixedColumns = withFixedColumns(ReactTable);

class JobAbandonmentCount extends React.Component {
  columns = [];
  constructor(props) {
    super(props);
    this.toLocalDate = this.toLocalDate.bind(this);
    this.reloadData = this.reloadData.bind(this);
    this.state = {
      data: props.abandonmentCount,
      show: true,
      date: {
        ge: null,
        le: null
      },
      columns: [
        {
          Header: "Date",
          accessor: "key",
          Cell: date => (
            <React.Fragment>{this.toLocalDate(date.value)}</React.Fragment>
          )
        },
        {
          Header: "Count",
          accessor: "value"
        }
      ]
    };
    this.handleClick = this.handleClick.bind(this);
  }

  toLocalDate(dateTime) {
    return moment(dateTime).format("MM/DD/YYYY");
  }

  handleStartDateChange(date) {
    // this.setState({ date: { ge: date, le: this.state.date.le } }, () =>
    //   this.reloadData()
    // );

    this.setState({
      date: {
        ge: date,
        le: this.state.date.le
      }
    });
  }

  handleEndDateChange(date) {
    // this.setState({ date: { ge: this.state.date.ge, le: date } }, () =>
    //   this.reloadData()
    // );

    this.setState({
      date: {
        ge: this.state.date.ge,
        le: date
      }
    });
  }

  getQuery() {
    const query = { filter: {} };
    if (this.state.date.ge) query.startDate = this.toLocalDate(this.state.date.ge);
    if (this.state.date.le) query.endDate = this.toLocalDate(this.state.date.le)
    return query;
  }

  reloadData() {
    var startDate = this.toLocalDate(this.state.date.ge);
    var endDate = this.toLocalDate(this.state.date.le);
    this.setState({ loading: true }, () =>
      CareerCircleAPI.getJobAbandonmentCount( startDate,endDate )
        .then(res => {
          this.setState({ data: res.abandonmentCount });
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
    this.reloadData();
  }

  render() {
    return (
      <div className="react-report-table">
        <div className="advanced-filters-wrapper p-2">
          {!this.state.show && (
            <button
              className="btn btn-primary ml-0 m-2 "
              onClick={this.handleClick}
            >
              Get Data
            </button>
          )}

          <div className="form-inline">
            <div className="form-group mr-1">
              <div className="d-block">
                <DatePicker
                  placeholderText="From Date"
                  selected={this.state.date.ge}
                  showYearDropdown
                  selectsStart
                  startDate={this.state.date.ge}
                  endDate={this.state.date.le}
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
                  selected={this.state.date.le}
                  showYearDropdown
                  selectsEnd
                  startDate={this.state.date.ge}
                  endDate={this.state.date.le}
                  isClearable={true}
                  onChange={date => this.handleEndDateChange(date)}
                  maxDate={new Date()}
                />
              </div>
            </div>
            <div className="form-group ml-1">
              <div className="d-block">
                <i onClick={this.handleClick} className="fas fa-search" />
              </div>
            </div>
          </div>
        </div>
        {this.state.show && (
          <ReactTableFixedColumns
            data={this.state.data}
            columns={this.state.columns}
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

export default JobAbandonmentCount;
