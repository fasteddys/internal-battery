import React from 'react';

class BrowseJobsByLocation extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            states: props.states
        };

        this.states = this.state.states.map((item) =>
            <div className="col-12 col-md-3" id={item.jobCategoryGuid}>{item.name}</div>
        );
    }

    render() {
        return (
            <div className="browse-jobs-container">
                <div className="row">
                    <div className="col-12">
                        <h2>Location</h2>
                    </div>
                    {this.states}
                </div>

            </div>
        );
    }
}

export default BrowseJobsByLocation;