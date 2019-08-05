import React, { Component } from "react";

export default class FailedSubscribers extends Component {
  constructor(props) {
    super(props);
  }

  renderList(item) {
    return (
      <div>
        <tr>
          <td>Jyoti</td>
          <td>Guin</td>
          <td className="text-right">1r</td>
        </tr>
      </div>
    );
  }
  render() {
    return (
      <div className="col-lg-3 col-md-3 col-sm-3 col-xs-3">
        <div className="card shadow mb-3">
          <div className="card-header">
            Failed Google Profile Index Subscribers
          </div>
          <div className="card-body p-0">
            <table className="table table-striped mb-0">
              <thead>
                <tr>
                  <th >First Name</th>
                  <th >Last Name</th>
                  <th className="text-right">
                    Data
                  </th>
                </tr>
                </thead>
                <tbody>ƒ
                <tr>
                    <td>Jyoti</td>
                    <td>Guin</td>
                    <td className="text-right">1r</td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>
    );
  }
}
