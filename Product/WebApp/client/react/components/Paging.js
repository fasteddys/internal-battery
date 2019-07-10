import React from 'react';

class Paging extends React.Component {
    constructor(props) {
        super(props);

    }

    render() {
        return (
            <div className="d-flex flex-row">
                    <i className="fas fa-arrow-left"></i>
                    <i className="col fas fa-arrow-right"></i>
            </div>
        )
    }

}
export default Paging;