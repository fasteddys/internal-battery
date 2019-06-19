import React from 'react';

const TextArea = props => {

    let formControl = "edit-option";

    if (props.touched && !props.valid) {
        formControl = 'edit-option control-error';
    }

    return (
        <div className={formControl}>
            <div className="input-label">{props.label}</div>
            <div className="edit-input">
                <textarea {...props} className="formControl" />
            </div>
        </div>
    );
}

export default TextArea;