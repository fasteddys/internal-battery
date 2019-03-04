import React from 'react';
import ReactDOM from 'react-dom';
import ContactSelect from './ContactSelect';

var contactSelectEle = document.querySelector('#contact-multi-select');
if(contactSelectEle)
    ReactDOM.render(<ContactSelect />, contactSelectEle);