import React from 'react';

class ContactMultiSelectRow extends React.Component {
    constructor(props) {
        super(props);
        this.props.selected = this.props.selected === undefined ? false : this.props.selected;
        this.handleClick = this.handleClick.bind(this);
    }

    handleClick() {
        this.props.onChange(this.props.contact.contactId, !this.props.selected);
    }

    render() {
        return (
            <tr key={this.props.contact.ContactId} onClick={this.handleClick}>
                <td>
                    <input class="form-check-input" type="checkbox" checked={this.props.selected} id="defaultCheck1"/>
                </td>
                <td>{this.props.contact.firstName} {this.props.contact.lastName}</td>
                <td>{this.props.contact.email}</td>
                <td>{this.props.contact.contactGuid}</td>
            </tr>
        );
    }
}

export default ContactMultiSelectRow;