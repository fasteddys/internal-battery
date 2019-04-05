import React from 'react';

class ResumeUpload extends React.Component {
    constructor(props) {
        super(props)
        this.state = {
            fileName: props.fileName,
            fileGuid: props.fileGuid,
            mode: props.fileGuid ? "view" : "new"
        };
    }

    render() {
        return (
            <div>
                <a className="download" href="/Home/DownloadFile?fileGuid={this.state.fileGuid}" target="_blank"><i className="fas fa-download" title="Download"></i> { this.state.fileName }</a>
                <button className="edit"><i className="fas fa-edit text-light-blue" title="Edit"></i></button>
                <button className="delete"><i className="fas fa-minus-circle text-light-blue" title="Delete"></i></button>
            </div>
        );
    }
}

export default ResumeUpload;