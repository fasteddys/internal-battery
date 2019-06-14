import React from 'react';

class Breadcrumb extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            breadcrumbList: props.breadcrumbObject.breadcrumbs.map((item, i) => {
                if (i !== props.breadcrumbObject.breadcrumbs.length - 1)
                        return <li key={i} className="breadcrumb-item"><a href={item.url}>{item.pageName}</a></li>
                    else
                        return <li key={i} className="breadcrumb-item active">{item.pageName}</li>
                })
        };
    }

    render() {
        return (
            <div className="breadcrumbs">
                <nav aria-label="breadcrumb">
                    <ol className="breadcrumb">
                        {this.state.breadcrumbList}
                    </ol>
                </nav>
            </div>
        );
    }
}

export default Breadcrumb;