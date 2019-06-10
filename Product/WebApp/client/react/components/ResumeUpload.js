import React from 'react';

class ResumeUpload extends React.Component {
    modeType = {
        upload: 0,
        view: 1,
        parseMerge : 2
    };

    constructor(props) {
        super(props)
        this.fileInput = React.createRef();
        let mergeFlag = false;
        if (props.resumeParseGuid != "")
            mergeFlag = true;

        this.state = {
            fileName: props.fileName,
            fileGuid: props.fileGuid,
            processing: false,
            mode: props.fileGuid ?  this.modeType.view : this.modeType.upload,
            selectedFile: null,
            resumeParseGuid: props.resumeParseGuid,
            requiresMerge: mergeFlag

        };
        this.onResumeMergeNeeded = this.onResumeMergeNeeded.bind(this);

    }

    onFileSelect(files) {
        this.setState({selectedFile: files[0]});
    }

    hasResume() {
        return this.state.fileGuid ? true : false;
    }


    onResumeMergeNeeded(e) {          
        this.setState({ resumeParseGuid: e.detail.resumeParseGuid });
        this.setState({ requiresMerge: e.detail.requiresMerge });            
    }

    delegateParseMerge() {    
        var evt = new CustomEvent('onDelegateParseMerge', { detail: this.state.resumeParseGuid });
        window.dispatchEvent(evt);
    }

    componentDidMount()
    {
        window.addEventListener('onResumeMergeNeeded', this.onResumeMergeNeeded);
    }

    changeMode(mode){
        this.setState({mode, selectedFile: null});
    }

    uploadResume(e) {
        e.preventDefault();
        if(!this.state.selectedFile)
            return;

        this.setState({processing: true}, () => {
            CareerCircleAPI.uploadResume(this.state.selectedFile, true)
                .then((response) => {
                    ToastService.success('Resume saved successfully.');
                    this.setState({mode: this.modeType.view, fileName: response.data.simpleName, fileGuid: response.data.subscriberFileGuid});
                })
                .catch((err) => {
                    ToastService.error('Unable to save resume.');
                })
                .finally(() => {
                    this.setState({processing: false});
                });
        });

    }

    deleteResume() {
        this.setState({processing: true}, () => {
            CareerCircleAPI.deleteFile(this.state.fileGuid)
                .then((response) => {
                    ToastService.success('Resume deleted successfully.');
                    this.setState({mode: this.modeType.upload, fileName: null, fileGuid: null, selectedFile: null});
                })
                .catch((err) => {
                    ToastService.error('Unable to delete resume.');
                })
                .finally(() => {
                    this.setState({processing: false});
                });
        });
    }

    spinner() {
        if(!this.state.processing)
            return;

        return  (<div className="d-inline">
            <span className="loading-spinner spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
            <span className="loading-spinner sr-only">Loading...</span>
        </div>);
    }

    viewMode() {

        if ( this.state.requiresMerge == true )
        return (
            <div>
                <a className="text-primary download-link pr-1" href={`/Home/DownloadFile?fileGuid=${this.state.fileGuid}`} target="_blank"><i className="fas fa-download" title="Download"></i> { this.state.fileName }</a>
                <button type="button" className="btn btn-text text-primary edit px-2" onClick={() => this.changeMode(this.modeType.upload)}><i className="fas fa-edit text-light-blue" title="Edit"></i></button>
                <button type="button" className="btn btn-text text-primary delete px-2" onClick={() => this.deleteResume()}><i className="fas fa-minus-circle text-light-blue" title="Delete"></i> {this.spinner()}</button>
                <button type="button" className="btn btn-text text-primary edit px-2" onClick={() => this.delegateParseMerge()}><i className="fas fa-exclamation text-warning" title="Your profile requires merging"></i></button>
            </div>
            )
        else 
            return (
                <div>
                    <a className="text-primary download-link pr-1" href={`/Home/DownloadFile?fileGuid=${this.state.fileGuid}`} target="_blank"><i className="fas fa-download" title="Download"></i> {this.state.fileName}</a>
                    <button type="button" className="btn btn-text text-primary edit px-2" onClick={() => this.changeMode(this.modeType.upload)}><i className="fas fa-edit text-light-blue" title="Edit"></i></button>
                    <button type="button" className="btn btn-text text-primary delete px-2" onClick={() => this.deleteResume()}><i className="fas fa-minus-circle text-light-blue" title="Delete"></i> {this.spinner()}</button>

                </div>
            )
    }

 

    uploadMode() {
        let cancelBtn;
        
        if(this.state.fileGuid != null)
            cancelBtn = <button type="button" onClick={() => this.changeMode(this.modeType.view)} className="btn btn-sm btn-secondary cancel">Cancel</button>;
        return (
            <form className="" method="post" action="/Home/UploadResume" encType="multipart/form-data">
                <div>
                    { this.state.selectedFile ? (
                    <div className="d-inline-block">
                        <label htmlFor="react-upload" className="pr-1">
                            <span className="file-name text-primary">{ this.state.selectedFile.name }</span>&nbsp;<i className="fas fa-exchange-alt text-primary"></i>
                        </label>
                        <button type="submit" onClick={(e) => this.uploadResume(e)} className="submit btn btn-sm btn-light-blue mr-2">
                            Save { this.spinner() }
                        </button>
                    </div>
                    )
                    :
                    (
                        <label htmlFor="react-upload" className="btn btn-sm btn-light-blue mb-0 mr-2">
                            <i className="fas fa-file-upload"></i>&nbsp;Upload Resume
                        </label>
                    )}

                    { cancelBtn }
                    <input required id="react-upload" className="pseudo-hidden file-input" type="file" onChange={(e) => this.onFileSelect(e.target.files)} accept=".doc,.docx,.odt,.pdf,.rtf,.tex,.txt,.wks,.wps,.wpd" />
                </div>
            </form>
        );
    }

    render() {  

        let display = null;
        if (this.state.mode == this.modeType.upload)
            display = this.uploadMode();
        else if (this.state.mode == this.modeType.parseMerge && this.state.requiresMerge == 1)
            display = this.parseMergeMode();
        else
            display = this.viewMode();

        return (<div className="resume-upload-component">
            {
               display   
            }
        </div>);
    }
}

export default ResumeUpload;