import React from 'react';
import Modal from 'react-modal';
import * as yup from 'yup';
// TODO - add react-quill when we are fully react.  It's won't work the reactjs.net due to it being only server side rendered in a razor view. 
 
 
 
 
const schema = yup.object().shape({
    title: yup.string().required()
        .min(10),  
    notification: yup.string().required()
        .min(10), 
    expireDate: yup.date().default(function () {
        return new Date();
    }),
});




class NotificationEditor extends React.Component {


 

    constructor(props) {
        super(props);
        this.expireDateRef = React.createRef();
 
        this.state = {  
            processing: false,
            titleValid: true,
            title: '', 
            notification: '',
            expireDate: '',
            notificationValid: true,
            expireDateValid: true,
            modelValid: false
        };       
     }


    onTitleChange(newVal) {
        this.setState({ title: newVal }) 
    }

    onNotificationChange(newVal) {
        this.setState({ notification: newVal })
    }

  
    validate() {

        var modelIsValid = true;
        if (yup.reach(schema, 'title').isValidSync(this.state.title) == false) {
            this.setState({ titleValid: false });
            modelIsValid = false;
        }            
        else 
            this.setState({ titleValid: true })

        if (yup.reach(schema, 'notification').isValidSync(this.state.notification) == false) {
            this.setState({ notificationValid: false });
            modelIsValid = false;
        }            
        else
            this.setState({ notificationValid: true })

        var expireDate = this.expireDateRef.current.value;
 
        if (yup.reach(schema, 'expireDate').isValidSync(expireDate) == false) {
            this.setState({ expireDateValid: false });
            modelIsValid = false;
        }
        else
            this.setState({ expireDateValid: true })

        this.setState({ modelValid: modelIsValid }) 
        if (modelIsValid == true)
            this.setState({ processing: true })

        return modelIsValid;
    }


    displayWait() {
        this.setState({ processing: true })
    }

    spinner() {

        if (!this.state.processing)
            return;

        return (<div className="d-inline"> 
            <span className="loading-spinner spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
            <span className="loading-spinner sr-only">Loading...</span>
        </div>);
    }

     
    render() {                
        return (
            <div>              
                <form action={"/Admin/CreateNotificationAsync"} method="post" id="NotificationEditorForm">   
                    <div className="form-group row">
                        <label for="Name" className="col-sm-2 col-form-label">Title:  </label>
                        <div className="col-sm-10">
                            <input type="text" className='form-control' name="Title"  onChange={e => this.onTitleChange(e.target.value)}/>
                            <div className={this.state.titleValid ? 'd-none' : 'invalid-feedback d-block'} >  Title is required     </div>
                        </div>

                    </div>
                     
                    <div className="form-group row">
                        <label for="title" className="col-sm-2 col-form-label">Notification:</label>
                        <div className="col-sm-10">
                            <textarea className="form-control" rows="3" name="Description" onChange={e => this.onNotificationChange(e.target.value)} ></textarea>
                            <div className={this.state.notificationValid ? 'd-none' : 'invalid-feedback d-block'} >  Notification is required     </div>
                        </div>
                    </div>
 

                    <div className="form-group row">
                        <label for="Expiration" className="col-sm-2 col-form-label">Expiration Date:</label>
                        <div className="col-sm-10">
                            <div className="input-group date">
                              
                                <input type="text" className="form-control datepicker begin-tomorrow" name="ExpirationDate" ref={this.expireDateRef} />
                                <div className={this.state.expireDateValid ? 'd-none' : 'invalid-feedback d-block'} >  Invalid expiration date    </div>
                                    <div className="input-group-addon">
                                        <span className="glyphicon glyphicon-th"></span>
                                    </div>
                                </div>
                                <small id="expirationDateHelpBlock" className="form-text text-muted">
                                    Date for which this notification is no longer active. Leave blank for no expiration date.
                                </small>
                            </div>
                    </div>
                    <div className="form-group row">
                        <div className="col-12 text-right">    
                            <button id="btnSubmit" form="NotificationEditorForm" type={this.state.modelValid ? "submit" : "button"} className="btn btn-primary" onClick={() => this.validate()} >Save {this.spinner()} </button>                   
                            <button id="btnSubmi1t" form="NotificationEditorForm" type="submit" className="btn btn-primary" onClick={() => this.validate()} >Plain Old Submit {this.spinner()} </button>     
                            </div>
                    </div>

                </form>
            </div>
 

        );
    }
}

export default NotificationEditor;