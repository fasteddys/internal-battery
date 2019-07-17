import React from 'react';
import Modal from 'react-modal';
import Select from '../Select';
import Validate from '../Validate';
import TextArea from '../TextArea';

const customStyles = {
    content: {
        top: '50%',
        left: '50%',
        right: 'auto',
        bottom: 'auto',
        marginRight: '-50%',
        transform: 'translate(-50%, -50%)',
        border: '1px solid #03264A'
    },
    overlay: {
        position: 'fixed',
        top: 0,
        left: 0,
        right: 0,
        bottom: 0,
        backgroundColor: 'rgba(0, 0, 0, 0.75)'
    }
};

class JobPostingAlert extends React.Component {
    constructor(props) {
        super(props);

        // set default description
        var defaultDescription = 'My job search alert for';
        if (props.jobQuery.keywords !== null && props.jobQuery.keywords !== '') {
            defaultDescription += " keyword '";
            defaultDescription += props.jobQuery.keywords;
            defaultDescription += "', ";
        }
        if (props.jobQuery.location !== null && props.jobQuery.location !== '') {
            defaultDescription += " location '";
            defaultDescription += props.jobQuery.location;
            defaultDescription += "' ";
        }
        if (defaultDescription.endsWith(", "))
            defaultDescription = defaultDescription.slice(0, -2);

        this.state = {
            timeZoneOffset: new Date().getTimezoneOffset(),
            localDate: new Date().toJSON(),
            modalIsOpen: false,
            jobQuery: props.jobQuery,
            showExecutionDayOfWeek: false,
            formIsValid: false,
            formControls: {
                description: {
                    value: defaultDescription,
                    label: 'Description',
                    placeholder: 'Enter a description for this job alert:',
                    valid: true,
                    validationRules: {
                        minLength: 10,
                        isRequired: true
                    },
                    touched: false
                },
                frequency: {
                    label: 'Delivery Schedule',
                    valid: false,
                    touched: true,
                    validationRules: {
                        isRequired: true,
                    },
                    options: [
                        { value: '', displayValue: 'Choose Weekly or Daily' },
                        { value: 'Weekly', displayValue: 'Weekly' },
                        { value: 'Daily', displayValue: 'Daily' }
                    ]
                },
                executionDayOfWeek: {
                    value: new Date().getDay(),
                    label: 'Delivery Day',
                    placeholder: 'Choose delivery day:',
                    valid: true,
                    touched: false,
                    validationRules: {
                        isRequired: true,
                    },
                    options: [
                        { value: 0, displayValue: 'Sunday' },
                        { value: 1, displayValue: 'Monday' },
                        { value: 2, displayValue: 'Tuesday' },
                        { value: 3, displayValue: 'Wednesday' },
                        { value: 4, displayValue: 'Thursday' },
                        { value: 5, displayValue: 'Friday' },
                        { value: 6, displayValue: 'Saturday' }
                    ]
                },
                executionHour: {
                    value: new Date().getHours(),
                    label: 'Delivery hour',
                    placeholder: 'Choose hour:',
                    valid: true,
                    touched: true,
                    validationRules: {
                        isRequired: true,
                    },
                    options: [
                        { value: 0, displayValue: '12 AM' },
                        { value: 1, displayValue: '1 AM' },
                        { value: 2, displayValue: '2 AM' },
                        { value: 3, displayValue: '3 AM' },
                        { value: 4, displayValue: '4 AM' },
                        { value: 5, displayValue: '5 AM' },
                        { value: 6, displayValue: '6 AM' },
                        { value: 7, displayValue: '7 AM' },
                        { value: 8, displayValue: '8 AM' },
                        { value: 9, displayValue: '9 AM' },
                        { value: 10, displayValue: '10 AM' },
                        { value: 11, displayValue: '11 AM' },
                        { value: 12, displayValue: '12 PM' },
                        { value: 13, displayValue: '1 PM' },
                        { value: 14, displayValue: '2 PM' },
                        { value: 15, displayValue: '3 PM' },
                        { value: 16, displayValue: '4 PM' },
                        { value: 17, displayValue: '5 PM' },
                        { value: 18, displayValue: '6 PM' },
                        { value: 19, displayValue: '7 PM' },
                        { value: 20, displayValue: '8 PM' },
                        { value: 21, displayValue: '9 PM' },
                        { value: 22, displayValue: '10 PM' },
                        { value: 23, displayValue: '11 PM' }
                    ]
                },
                executionMinute: {
                    value: Math.min(Math.ceil(new Date().getMinutes() / 5) * 5, 55),
                    label: 'Delivery minute',
                    placeholder: 'Choose minute:',
                    valid: true,
                    touched: true,
                    validationRules: {
                        isRequired: true,
                    },
                    options: [
                        { value: 0, displayValue: ':00' },
                        { value: 5, displayValue: ':05' },
                        { value: 10, displayValue: ':10' },
                        { value: 15, displayValue: ':15' },
                        { value: 20, displayValue: ':20' },
                        { value: 25, displayValue: ':25' },
                        { value: 30, displayValue: ':30' },
                        { value: 35, displayValue: ':35' },
                        { value: 40, displayValue: ':40' },
                        { value: 45, displayValue: ':45' },
                        { value: 50, displayValue: ':50' },
                        { value: 55, displayValue: ':55' }
                    ]
                }
            }
        };

        this.openModal = this.openModal.bind(this);
        this.afterOpenModal = this.afterOpenModal.bind(this);
        this.closeModal = this.closeModal.bind(this);
    }

    openModal(e) {
        e.preventDefault();
        this.setState({ modalIsOpen: true });
    }

    afterOpenModal() {
        // todo: set a default description based on the keywords and location chosen
    }

    closeModal() {
        this.setState({ modalIsOpen: false });
    }

    changeHandler = event => {

        const name = event.target.name;
        const value = event.target.value;

        const updatedControls = {
            ...this.state.formControls
        };
        const updatedFormElement = {
            ...updatedControls[name]
        };
        updatedFormElement.value = value;
        updatedFormElement.touched = true;
        updatedFormElement.valid = Validate(value, updatedFormElement.validationRules);

        updatedControls[name] = updatedFormElement;

        let formIsValid = true;
        for (let inputIdentifier in updatedControls) {
            formIsValid = updatedControls[inputIdentifier].valid && formIsValid;
        }

        this.setState({
            formControls: updatedControls,
            formIsValid: formIsValid,
            showExecutionDayOfWeek: (name == 'frequency' && value == 'Daily') ? false : true
        });
    }

    formSubmitHandler = () => {
        const formData = {};
        for (let formElementId in this.state.formControls) {
            formData[formElementId] = this.state.formControls[formElementId].value;
        }

        // todo: how best to handle a user that is not authenticated? 
        var self = this;
        CareerCircleAPI.addJobAlert(this.state.jobQuery, formData.description, formData.frequency, formData.executionHour, formData.executionMinute, formData.executionDayOfWeek, this.state.timeZoneOffset, this.state.localDate)
            .then(function (response) { 
                ToastService.success("Manage your job alerts in 'My Job Alerts'", "Job alert created successfully!");
                self.closeModal();
            })
            .catch((error) => {
                if (error.response && error.response.data)
                    ToastService.error(error.response.data.description, "Unable to create job alert");
            })
    }

    render() {
        return (
            <div>
                <button id="CreateAlert" className="btn btn-primary" onClick={this.openModal}><i className="far fa-bell"></i> Create Job Alert</button>
                <Modal
                    isOpen={this.state.modalIsOpen}
                    onAfterOpen={this.afterOpenModal}
                    onRequestClose={this.closeModal}
                    style={customStyles}
                >
                    <div className="form-row job-alert-modal">
                        <div className="job-alert-header">
                            <h4>Create Job Alert</h4>
                        </div>
                        <TextArea name="description"
                            label={this.state.formControls.description.label}
                            placeholder={this.state.formControls.description.placeholder}
                            value={this.state.formControls.description.value}
                            onChange={this.changeHandler}
                            touched={this.state.formControls.description.touched}
                            valid={this.state.formControls.description.valid}
                        />
                        <Select name="frequency"
                            label={this.state.formControls.frequency.label}
                            value={this.state.formControls.frequency.value}
                            onChange={this.changeHandler}
                            options={this.state.formControls.frequency.options}
                            touched={this.state.formControls.frequency.touched}
                            valid={this.state.formControls.frequency.valid}
                        />
                        {this.state.showExecutionDayOfWeek ? <Select name="executionDayOfWeek"
                            label={this.state.formControls.executionDayOfWeek.label}
                            value={this.state.formControls.executionDayOfWeek.value}
                            onChange={this.changeHandler}
                            options={this.state.formControls.executionDayOfWeek.options}
                            touched={this.state.formControls.executionDayOfWeek.touched}
                            valid={this.state.formControls.executionDayOfWeek.valid}
                        /> : null}
                        <Select name="executionHour"
                            label={this.state.formControls.executionHour.label}
                            value={this.state.formControls.executionHour.value}
                            onChange={this.changeHandler}
                            options={this.state.formControls.executionHour.options}
                            touched={this.state.formControls.executionHour.touched}
                            valid={this.state.formControls.executionHour.valid}
                        />
                        <Select name="executionMinute"
                            label={this.state.formControls.executionMinute.label}
                            value={this.state.formControls.executionMinute.value}
                            onChange={this.changeHandler}
                            options={this.state.formControls.executionMinute.options}
                            touched={this.state.formControls.executionMinute.touched}
                            valid={this.state.formControls.executionMinute.valid}
                        />
                    </div>
                    <div className="job-alert-footer">
                        <button className="btn btn-secondary" onClick={this.closeModal}>Cancel</button>
                        <button className="btn btn-primary" onClick={this.formSubmitHandler} disabled={!this.state.formIsValid}>Save</button>
                    </div>
                </Modal>
            </div>
        );
    }
}

export default JobPostingAlert;