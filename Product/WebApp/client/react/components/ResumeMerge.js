import React from 'react';
 

class ResumeMerge extends React.Component {

    constructor(props) {
        super(props)
     
        this.onDoParseMerge = this.onDoParseMerge.bind(this);

    }

    onDoParseMerge(e) {
        console.log("onDoParseMerge detail = " + e.detail);            
        CareerCircleAPI.getResumeParseMerge(e.detail)
            .then((response) => {
                alert(response)
            })
            .catch((err) => {
                ToastService.error('Unable too locate profile merge data.');
            });
    }


    componentDidMount() {
        window.addEventListener('onDoParseMerge', this.onDoParseMerge);
    }

    render() {
        return (
            <div className="modal fade" id="ResumeMergeModal" tabIndex="-1" role="dialog" aria-hidden="true">
                <div className="modal-dialog" role="document">
                    <div className="profile-edit-modal-header-container">
                        <h4>Merge profiel</h4>
                    </div>
                    <div className="modal-content">
                        <div className="modal-body">
                            <div className="modal-footer">
                                <button type="button" className="btn btn-secondary" data-dismiss="modal">Close</button>                           
                            </div>
                        </div>
                    </div>
                </div>
  
            </div>
        );
    }
}

export default ResumeMerge;