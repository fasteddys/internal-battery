import React from 'react';
import ReactDOM from 'react-dom';
import ContactMultiSelect from './ContactMultiSelect';
import TableMultiSelect from './TableMultiSelect';

var contactMultiSelectEl = document.querySelector('#contact-multi-select');
if(contactMultiSelectEl)
    ReactDOM.render(<TableMultiSelect />, contactMultiSelectEl);