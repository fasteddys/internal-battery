import React from 'react';

class BrowseJobs extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            list: props.list,
            header: props.header
        };
        this.locationList = this.state.list.map((item) => (
            <div className="col-12 col-md-6 col-lg-4">
                <div className="job-type-listing shadow-2">
                    <a href={item.url}>
                        <div className="row">
                            <div className="col-9">
                                <strong>{item.label}</strong>
                            </div>
                            <div className="col-3 container-align-center">
                                <strong>{item.count}</strong>
                            </div>
                        </div>
                    </a>
                </div>
                
            </div>
        ));
    }

    render() {
        return (
            <div className="browse-jobs-container bounceInRight animated">
                <div className="row">
                    <div className="col-12">
                        <h1>{this.state.header}</h1>
                    </div>
                    {this.locationList}
                </div>

            </div>
        );
    }
}

export default BrowseJobs;