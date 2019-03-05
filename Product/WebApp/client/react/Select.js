import React from 'react';

class Select extends React.Component {

    constructor(props){
        super(props);
    }

    renderOptions() {
        const options = [];
        this.props.options.forEach(option => options.push(this.props.accessor(option)));
        return options.map(option => <option key={option.value} value={option.value}>{option.label}</option>);
    }

    render() {
        return (
            <select className="form-control" onChange={(event) => this.props.onChange(event.target.value)} value={this.props.value}>
                <option>None</option>
                {this.renderOptions()}
            </select>
        );
    }
}

export default Select;