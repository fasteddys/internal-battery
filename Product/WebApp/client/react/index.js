import React from 'react';
import ReactDOM from 'react-dom';
import ContactSelect from './ContactSelect';
import Counter from './counter/Counter';

var contactSelectEle = document.querySelector('#contact-multi-select');
if(contactSelectEle)
    ReactDOM.render(<ContactSelect />, contactSelectEle);

var counterEle = document.querySelector('#counter');
if(counterEle)
    ReactDOM.render(<Counter/>, counter);

global.Counter = Counter;