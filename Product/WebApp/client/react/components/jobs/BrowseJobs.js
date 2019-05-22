import React from 'react';

class BrowseJobs extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            list: props.list
        };
        this.locationList = this.state.list.map((item) => (
            <div className="col-12 col-md-3">
                <a href={item.url}>
                    {item.label}
                </a>
            </div>
        ));
    }

    render() {
        return (
            <div className="browse-jobs-container">
                <div className="row">
                    <div className="col-12">
                        <h2>Location</h2>
                    </div>
                    {this.locationList}
                </div>

            </div>
        );
    }
}

export default BrowseJobs;