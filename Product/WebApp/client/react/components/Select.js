import React from 'react';

const Select = props => {

    let formControl = "edit-option";

    if (props.touched && !props.valid) {
        formControl = 'edit-option control-error';
    }

    return (
        <div className={formControl}>
            <div className="input-label">{props.label}</div>
            <div className="edit-input">
                <select className="form-control" value={props.value} onChange={props.onChange} name={props.name}>
                    {props.options.map(option => (
                        <option value={option.value}>
                            {option.displayValue}
                        </option>
                    ))}
                </select>
            </div>
        </div>
    );
}

export default Select;