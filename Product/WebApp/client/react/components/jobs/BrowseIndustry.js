import React from 'react';

class BrowseJobsByIndustry extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            industries: props.industries
        };

        this.industries = this.state.industries.map((item) =>
            <div className="col-12 col-md-6" id={item.jobCategoryGuid}>{item.name}</div>
        );
    }

    render() {
        return (
            <div className="browse-jobs-container">
                <div className="row">
                    <div className="col-12">
                        <h2>Industries</h2>
                    </div>
                    {this.industries}
                </div>

            </div>
        );
    }
}

export default BrowseJobsByIndustry;