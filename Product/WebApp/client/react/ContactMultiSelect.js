import React from 'react';
import ContactMultiSelectRow from './ContactMultiSelectRow';

class ContactMultiSelect extends React.Component {
    constructor(props) {
        super(props);
        this.state = { contacts: null, selectedContacts: {}, selectAll: false, pageIndex: 1, pageSize: 3};

        CareerCircleAPI.getContacts().then((res) => {
            this.setState({ contacts: res.data.data });
        });

        this.handleCheckClick = this.handleCheckClick.bind(this);
        this.handleSelectAllClick = this.handleSelectAllClick.bind(this);
    }

    handleSelectAllClick() {
        this.setState(state => {
            state.selectAll = !state.selectAll;
            state.selectedContacts = {};
            return state;
        });
    }

    handleCheckClick(contactId, selected) {
        var selectedContacts = this.state.selectedContacts;

        if((selected && !this.state.selectAll) || (this.state.selectAll && !selected))
            selectedContacts[contactId] = true;
        else
            delete selectedContacts[contactId];
        
        this.setState({selectedContacts: selectedContacts});
    }

    isContactSelected(contactId) {
        if(this.state.selectedContacts[contactId] === undefined) {
            return this.state.selectAll;
        }

        if(this.state.selectAll)
            return !this.state.selectedContacts[contactId];

        return this.state.selectedContacts[contactId] || this.state.selectAll;
    }

    renderContacts() {
        let contactsList = this.state.contacts.map(contact => {
            return (
                <ContactMultiSelectRow contact={contact} selected={this.isContactSelected(contact.contactId)} onChange={this.handleCheckClick}/>
            );
        })
        return contactsList;
    }

    render() {
        if(!this.state.contacts)
            return (
                <div class="d-flex align-items-center">
                    <strong>Loading...</strong>
                    <div class="spinner-border ml-auto" role="status" aria-hidden="true"></div>
                </div>
            )

        return (
            <div>
                <table className="table">
                    <thead>
                        <tr>
                            <th>
                                <div>
                                    <input class="form-check-input" type="checkbox" checked={this.state.selectAll} id="select-all" onClick={this.handleSelectAllClick}/>
                                </div>
                            </th>
                            <th>Name</th>
                            <th>Email</th>
                            <th>Contact Guid</th>
                        </tr>
                    </thead>
                    <tbody>
                        {this.renderContacts()}
                    </tbody>
                </table>
                <div>
                    <button class="btn btn-sm display-inline">Previous</button>
                    <span>{ this.pageIndex }</span>
                    <button class="btn btn-sm display-inline">Next</button>
                </div>
            </div>
        );
    }
}

export default ContactMultiSelect;