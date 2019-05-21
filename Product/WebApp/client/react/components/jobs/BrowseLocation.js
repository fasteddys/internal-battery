import React from 'react';

class BrowseJobsByLocation extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            locations: props.cities
        };

        this.locations = this.state.locations.map((item) =>
            <div className="col-12 col-md-3">{item.label} ({item.count})</div>
        );
    }

    render() {
        return (
            <div className="browse-jobs-container">
                <div className="row">
                    <div className="col-12">
                        <h2>Location</h2>
                    </div>
                    {this.locations}
                </div>

            </div>
        );
    }
}

export default BrowseJobsByLocation;