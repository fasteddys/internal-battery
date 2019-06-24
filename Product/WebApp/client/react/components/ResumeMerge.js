import React from 'react';
 
 

// Local componenet for rendering skill inputs 
const SkillCheckbox = ({resultGuid, parsedValue, checked, onChange  }) => (
        <input type="checkbox" name={"chk_" + resultGuid} id={"chk_" + resultGuid} value={parsedValue} checked={checked}    onChange={(e) => onChange(e)} />  
);


const  SkillScrollableDivStyle = { maxHeight: "300px", overflow : "scroll", position : "relative" }
const SkillScrollMoreStyle = {
    textAlign: "center", backgroundColor: "#007bff", opacity: "0.9", color: "white", borderRadius: "3"}



class ResumeMerge extends React.Component {
 
    constructor(props) {
        super(props)
        this.state = { 
            processing: false,
            questions: null,
            hasQuestions: false,
            selectAllSkills : false,
            skillChecks : null,
            selectAllSkillsLabel: "Select All",
            displayScrollMessage : true  
        };
        this.onDoParseMerge = this.onDoParseMerge.bind(this);      
        this.onSkillChange = this.onSkillChange.bind(this);   
       
}


 
    spinner() { 

        if (!this.state.processing)
            return;

        return (<div className="d-inline">
            <span className="loading-spinner spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
            <span className="loading-spinner sr-only">Loading...</span>
        </div>);
    }



    selectAllSkills()
    {        
 
       if ( this.state.selectAllSkills  == false  )
            this.setState({ selectAllSkillsLabel: "Select All" });
       else 
            this.setState({ selectAllSkillsLabel: "Unselect All" });

        let skillChecks = this.state.skillChecks;

        skillChecks.forEach(q => {        
          q.checked = this.state.selectAllSkills;
        })
        this.forceUpdate();
  

    }

    onSelectAllSkills()
    { 
       this.setState({selectAllSkills: !this.state.selectAllSkills},
       this.selectAllSkills) 
    }
 
    onScrollSkills(e)
    {
        var ctl = $("#divSkills")[0];  
        if (ctl.scrollHeight <= $("#divSkills").height()) {
            this.setState({ displayScrollMessage: false });
        }
        else {
            var showScrollMessage = (ctl.scrollTop + $("#divSkills").height()) != ctl.scrollHeight;
            this.setState({ displayScrollMessage: showScrollMessage });
            // show the scroll message here since OnComponentUpdate will fade it out
            $("#divScrollMessage").show();
        }
 
    }

    onDoParseMerge(e) {      
        CareerCircleAPI.getResumeParseMergeQuestionnaire(e.detail)
            .then((response) => {
                this.setState({ questions: response });
                this.setState({ hasQuestions: true });

                let skillChecks = response.data.skills.map((o, index) =>
                (
                   {
                       resultGuid:  o.resumeParseResultGuid,
                       parsedValue: o.parsedValue,
                       checked: false,
                       existingValue: o.existingValue                                     
                   }
                     
                ))

                this.setState({ skillChecks: skillChecks });
                $("#ResumeMergeModal").modal();

                // hide the scroll message for 6 or less skills since
                // they will show without scrolling
                if (skillChecks != null && skillChecks.length <= 6)
                    this.setState({ displayScrollMessage: false });

             
         
            })
            .catch((err) => {
                ToastService.error('Unable to locate profile merge data.');
            });
    }


    formatQuestions(questions) {
        let rval = <div className="profile-edit-modal-header-container">
            <ul className="list-group">
                {
                    questions.map((o, index) =>
                        (
                            <li className="list-group-item border-0" key={o.resumeParseResultGuid}>
                                <div> {o.prompt} </div>
                                <div className="ml-3">
                                    <input type="radio" name={"rb_" + o.resumeParseResultGuid} id={"rb_existing_" + o.resumeParseResultGuid}  value="existing" defaultChecked />  {o.existingValue}
                                </div>
                                <div className="ml-3">
                                    <input type="radio" name={"rb_" + o.resumeParseResultGuid} id={"rb_parsed_" + o.resumeParseResultGuid} value="parsed" />  {o.parsedValue}
                                </div>
                                <div className="ml-3">
                                    <input type="radio" name={"rb_" + o.resumeParseResultGuid} id={"rb_neither_" + o.resumeParseResultGuid}  value="neither" />  Neither
                                </div>
                            </li>
                        ))
                }
            </ul>
        </div>
        return rval;
    }


    formatRadioQuestionHeader(hasRadioQuestions) {
        return hasRadioQuestions == true ? <div className="profile-edit-modal-header-container"> <h5>  Please clarify the following: </h5> </div>  : null 
    }

   formatSkillsScrollMessage()
   {
       return this.state.displayScrollMessage ?  <div className="p-1 rounded" style={SkillScrollMoreStyle} id="divScrollMessage" > Scroll for more skills... </div>  : null
   }


    onSkillChange(e)
    { 
        let skillChecks = this.state.skillChecks
        skillChecks.forEach(q => {
        if (q.existingValue === event.target.value)
            q.checked =  event.target.checked
        })
        this.setState({skillChecks: skillChecks})
    }


    formatSkills(skills) {

        let rval =<div> 
            <div className="profile-edit-modal-header-container" style={SkillScrollableDivStyle} id="divSkills"  onScroll={(e) => this.onScrollSkills(e)}  >
            <h5> Which of the following skills do you have? </h5>
            <div className = "p-2">     <input type="checkbox" name="chkSelectAllParsedSkills" id="chkSelectAllParsedSkills" onClick={(e) => this.onSelectAllSkills(e)} />  {this.state.selectAllSkillsLabel}  </div>
            <ul className="list-group ">
                {
                    this.state.skillChecks.map((o, index) =>
                        (
                            <li className="list-group-item border-0 p-0 " key={o.resultGuid}>
                                <div className="ml-3 h6">

                                <SkillCheckbox resultGuid={o.resultGuid}  parsedValue={o.parsedValue} existingValue={o.existingValue} checked={o.checked} onChange={this.onSkillChange} />             <label>     {o.existingValue}   </label>         

                                </div>
                            </li>
                        ))
                }
            </ul>           
        </div>       
        { this.formatSkillsScrollMessage() } 
        </div>
        return rval;
    }
 
    displayWait() {
        this.setState({ processing: true })
    }

    componentDidMount() {
        window.addEventListener('onDoParseMerge', this.onDoParseMerge);   
    }

    componentDidUpdate() {
        window.addEventListener('onDoParseMerge', this.onDoParseMerge);
        // fade the message our here since it will always be initally shown, even when
        // there are not enough skill to warrant scrolling 
        $("#divScrollMessage").fadeOut(5000);
 


    }


    render() {
 
        let contactQuestions = null;
        let workHistoryQuestions = null;
        let educationHistoryQuestions = null;
        let skillQuestions = null;
        let parseGuid = "";
        let hasRadioQuestions = false;

        if ( this.state.hasQuestions == true )
            parseGuid = this.state.questions.data.resumeParseGuid;

        if (this.state.hasQuestions == true && this.state.questions.data.contactQuestions.length > 0) {
            contactQuestions = this.formatQuestions(this.state.questions.data.contactQuestions);  
            hasRadioQuestions = true;
        }
            

        if (this.state.hasQuestions == true && this.state.questions.data.workHistoryQuestions.length > 0) {
            workHistoryQuestions = this.formatQuestions(this.state.questions.data.workHistoryQuestions);  
            hasRadioQuestions = true;
        }
            

        if (this.state.hasQuestions == true && this.state.questions.data.educationHistoryQuestions.length > 0) {
            educationHistoryQuestions = this.formatQuestions(this.state.questions.data.educationHistoryQuestions);
            hasRadioQuestions = true;
        }
            

        if (this.state.hasQuestions == true && this.state.questions.data.skills.length > 0 && this.state.skillChecks != null) {

            skillQuestions = this.formatSkills(this.state.questions.data.skills); 
        }
            
  
        return (
            <form action={"/Home/resume-merge/" + parseGuid}  method="post" id="ResumeMergeForm">   
            <div className="modal fade" id="ResumeMergeModal" tabIndex="-1" role="dialog" aria-hidden="true">
                <div className="modal-dialog" role="document">                
                    <div className="modal-content">
                         <div className="modal-body">
                            {
                               this.formatRadioQuestionHeader(hasRadioQuestions)
                            }                            
                            {
                                contactQuestions
                            }
                            {
                                workHistoryQuestions
                            }
                            {
                                educationHistoryQuestions
                            }
                            {
                                skillQuestions
                            }

                            <div className="modal-footer">

                                    <button type="button" className="btn btn-secondary" data-dismiss="modal">Close</button>     
                                    <button id="btnResumeMerge" form="ResumeMergeForm" type="submit" className="btn btn-primary" onClick={() => this.displayWait()} >Save {this.spinner()} </button>
                            </div>
                        </div>
                    </div>
                </div>
  
                </div>
                </form>
        );
    }
}

export default ResumeMerge;