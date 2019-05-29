import React from 'react';

class BrowseJobs extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            list: props.list,
            header: props.header,
            baseUrl: props.baseUrl + "/1",
            hideAllLink: props.hideAllLink
        };
        this.locationList = this.state.list.map((item) => (
            <div className="col-12 col-md-6 col-lg-4 no-padding">
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
        let allLink;

        if (!this.props.hideAllLink) {
            allLink = (
                <div className="row">
                    <div className="col-12 col-md-6 col-lg-4 no-padding">
                        <div className="job-type-listing shadow-2">
                            <a href={this.state.baseUrl}>
                                <div className="row">
                                    <div className="col-12 container-align-center">
                                        <strong>All</strong>
                                    </div>
                                </div>
                            </a>
                        </div>
                    </div>
                </div>
            );
        }
        return (
            <div className="browse-jobs-container bounceInRight animated">
                <h1><strong>{this.state.header}</strong></h1>
                <div className="row">
                    {this.locationList}
                </div>
                {allLink}
                

            </div>
        );
    }
}

export default BrowseJobs;