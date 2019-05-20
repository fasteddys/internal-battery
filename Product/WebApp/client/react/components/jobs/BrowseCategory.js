import React from 'react';

class BrowseJobsByCategory extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            jobCategories: props.jobCategories
        };

        this.categories = this.state.jobCategories.map((item) =>
            <div className="col-12 col-md-6" id={item.jobCategoryGuid}>{item.name}</div>
        );
    }

    render() {
        return (
            <div className="browse-jobs-container">
                <div className="row">
                    {this.categories}
                </div>
                
            </div>
        );
    }
}

export default BrowseJobsByCategory;