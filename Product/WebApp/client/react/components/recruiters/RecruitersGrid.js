import React from 'react';
import Paper from '@material-ui/core/Paper';
import Chip from '@material-ui/core/Chip';
import Input from '@material-ui/core/Input';
import Select from '@material-ui/core/Select';
import TextField from '@material-ui/core/TextField';
import MenuItem from '@material-ui/core/MenuItem';
import CircularProgress from '@material-ui/core/CircularProgress';

import IconButton from '@material-ui/core/IconButton';

import DeleteIcon from '@material-ui/icons/Delete';
import EditIcon from '@material-ui/icons/Edit';
import SaveIcon from '@material-ui/icons/Save';
import CancelIcon from '@material-ui/icons/Cancel';

import {
    DataTypeProvider,
    EditingState,
    PagingState,
    IntegratedPaging,
    SearchState,
    IntegratedFiltering,
    SortingState,
    IntegratedSorting
} from '@devexpress/dx-react-grid';
import {
    Grid,
    Table,
    Toolbar,
    SearchPanel,
    TableHeaderRow,
    TableEditRow,
    TableEditColumn,
    PagingPanel,
} from '@devexpress/dx-react-grid-material-ui';

const EditButton = ({ onExecute }) => (
    <IconButton onClick={onExecute} title="Edit row">
        <EditIcon />
    </IconButton>
);

const DeleteButton = ({ onExecute }) => (
    <IconButton
        onClick={() => {
            // eslint-disable-next-line
            if (window.confirm('Are you sure you want to delete this row?')) {
                onExecute();
            }
        }}
        title="Delete row"
    >
        <DeleteIcon />
    </IconButton>
);

const CommitButton = ({ onExecute }) => (
    <IconButton onClick={onExecute} title="Save changes">
        <SaveIcon />
    </IconButton>
);

const CancelButton = ({ onExecute }) => (
    <IconButton color="secondary" onClick={onExecute} title="Cancel changes">
        <CancelIcon />
    </IconButton>
);

const commandComponents = {
    edit: EditButton,
    delete: DeleteButton,
    commit: CommitButton,
    cancel: CancelButton,
};

const Command = ({ id, onExecute }) => {
    const CommandButton = commandComponents[id];
    return (
        <CommandButton
            onExecute={onExecute}
        />
    );
};

const LoadingState = ({ loading, columnCount }) => (
    <td colSpan={columnCount} style={{ textAlign: 'center', verticalAlign: 'middle' }}>
        <big>
            {loading ? (<CircularProgress size={28} />) : (<span>No data</span>)}
        </big>
    </td>
)

class RecruitersGrid extends React.PureComponent {

    constructor(props) {
        super(props);

        this.state = {
            columns: [
                { name: 'FirstName', title: 'First Name', dataType: 'string' },
                { name: 'LastName', title: 'Last Name', dataType: 'string' },
                { name: 'Email', title: 'Email', dataType: 'string' },
                { name: 'Phone', title: 'Phone', dataType: 'int' },
            ],
            rows: [],
            pageSizes: [5, 10, 15, 0],
            searchValue: '',
            editingRowIds: [],
            sorting: [],
            rowChanges: [],
            loading: true,
            searchRecruiter: '',
            recruiter: {
                firstName: '',
                lastName: '',
                email: '',
                phone: '',
                isRecruiter: true,
                subscriberGuid:'',
            },
        };

        this.changeSearchValue = value => this.setState({ searchValue: value });
        this.changeSorting = sorting => this.setState({ sorting });
        this.changeEditingRowIds = this.changeEditingRowIds.bind(this);
        this.commitChanges = this.commitChanges.bind(this);
        this.invalidEditedRowIds = [];
        this.searchRecruiters = this.searchRecruiters.bind(this);
        this.changePermissions = this.changePermissions.bind(this);
        this.addRecruiter = this.addRecruiter.bind(this);
    }

    componentDidMount() {
        this.getRecruiters();
    }

    getRecruiters() {
        CareerCircleAPI.getRecruiters()
            .then((res) => {
                this.setState({ rows: this.buildRows(res.data) })
            }).catch(() => ToastService.error('An error occured while loading the recruiters.'))
            .finally(() => {
                this.setState({ loading: false });
            })
    }

    buildRows(data) {
        let rowData = [];
        data.forEach(x => rowData.push(
            {
                FirstName: x.firstName,
                LastName: x.lastName,
                Email: x.email,
                Phone: x.phoneNumber
            }));

        return rowData;
    }

    changeEditingRowIds(editingRowIds) {
        // Update state according to invalid edited rows
        if (this.invalidEditedRowIds.length) {
            this.setState({ editingRowIds: [...this.invalidEditedRowIds] });
            this.invalidEditedRowIds.length = 0;
        } else {
            this.setState({ editingRowIds });
        }
    }

    commitChanges({ added, changed, deleted }) {
        if (added) { }
        if (changed) { }
        if (deleted) { }
    }

    searchRecruiters(e) {
        e.preventDefault();
        var searchValue = this.searchRecruiter.value;

        const query = { filter: {} };

        //email pattern
        var emailPattern = /^\w+@[a-zA-Z_]+?\.[a-zA-Z]{2,3}$/;

        if (searchValue && searchValue.match(emailPattern)) {
            query.filter.Email = searchValue;
            CareerCircleAPI.getRecruiterDetails(query)
                .then((res) => {
                    if (res.statusText == 'OK') {
                        this.setState({ recruiter: this.buildRecruiter(res.data) })
                    }
                })
                .catch(() => ToastService.error('An error occured while loading the recruiter.'))
        }
        else {
            ToastService.error('Invalid Email id')
        }


    }

    buildRecruiter(data) {
        let recruiterData = {
            firstName: data.firstName,
            lastName: data.lastName,
            email: data.email,
            phone: data.phoneNumber,
            subscriberGuid: data.subscriberGuid,
            isRecruiter: this.state.recruiter.isRecruiter
        };

        return recruiterData;
    }

    changePermissions() {
        this.setState({
            recruiter: {
                firstName: this.state.recruiter.firstName,
                lastName: this.state.recruiter.lastName,
                email: this.state.recruiter.email,
                phone: this.state.recruiter.phoneNumber,
                subscriberGuid: this.state.recruiter.subscriberGuid,
                isRecruiter: !this.state.recruiter.isRecruiter
            }
        })
    }

    addRecruiter(e) {
        e.preventDefault();

        if (this.state.recruiter != undefined && this.state.recruiter != null) {
            if (this.state.recruiter.email != undefined && this.state.recruiter.email != null) {

                CareerCircleAPI.addRecruiter((this.state.recruiter))
                    .then((res) => {
                        //this.setState({ data: res.data });
                    })
                    .catch((err) => ToastService.error('An error occured while adding the recruiter.'))
            }
        }
        //var searchValue = this.searchRecruiter.value;

        //const query = { filter: {} };

        ////email pattern
        //var emailPattern = /^\w+@[a-zA-Z_]+?\.[a-zA-Z]{2,3}$/;

        //if (searchValue && searchValue.match(emailPattern)) {
        //    query.filter.Email = searchValue;
        //}

        //CareerCircleAPI.getRecruiterDetails(query)
        //    .then((res) => {
        //        //this.setState({ data: res.data });
        //    })
        //    .catch((err) => ToastService.error(err/*'An error occured while loading the recruiter.'*/))
        //    .finally(() => {
        //        //this.setState({ loading: false });
        //    })
    }

    render() {
        const { rows, columns, pageSizes, searchValue, sorting, editingRowIds, loading, recruiter, isRecruiter } = this.state;

        return (
            <div>
                {/*Add Recruiter*/}
                <div class="modal fade" id="addRecruiterModal" tabindex="-1" role="dialog" aria-hidden="true">
                    <div class="modal-dialog" role="document">
                        <div class="modal-content">
                            <div name="RecruiterForm">
                                <div class="modal-header">
                                    <h5 class="modal-title" id="exampleModalLabel"><b>Recruiter</b></h5>
                                    <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                                        <span aria-hidden="true">&times;</span>
                                    </button>
                                </div>
                                <div class="modal-body">
                                    <div class="row">
                                        <div class="col-6">
                                            <input id="searchRecruitersBox" type="text" placeholder="Search by Email" ref={(value) => this.searchRecruiter = value} />
                                        </div>
                                        <div class="col-4">
                                            <button id="searchRecruitersButton" type="button" onClick={this.searchRecruiters} class="btn btn-primary">Search</button>
                                        </div>
                                    </div>

                                    <div class="row">
                                        <div class="col-12">
                                            <div class="form-group">
                                                <div class="row">
                                                    <div class="col-4">
                                                        <label for="firstName">First Name :</label>
                                                    </div>
                                                    <div class="col-4">
                                                        <input type="text" id="firstName" value={recruiter.firstName} />
                                                    </div>
                                                </div>
                                                <div class="row">
                                                    <div class="col-4">
                                                        <label for="lastName">Last Name :</label>
                                                    </div>
                                                    <div class="col-4">
                                                        <input type="text" id="lastName" value={recruiter.lastName} />
                                                    </div>
                                                </div>
                                                <div class="row">
                                                    <div class="col-4">
                                                        <label for="email">Email</label>
                                                    </div>
                                                    <div class="col-4">
                                                        <input type="text" id="email" disabled value={recruiter.email} />
                                                    </div>
                                                </div>
                                                <div class="row">
                                                    <div class="col-4">
                                                        <label for="phone">Phone</label>
                                                    </div>
                                                    <div class="col-4">
                                                        <input type="text" id="phone" value={recruiter.phone} />
                                                    </div>
                                                </div>
                                                <div class="row">
                                                    <div class="col-4">
                                                        <label for="isRecruiter">Is Recruiter</label>
                                                    </div>
                                                    <div class="col-4">
                                                        <input type="checkbox" id="isRecruiter" checked={recruiter.isRecruiter} onChange={this.changePermissions} />
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>

                                    <div class="row">
                                        <div class="col-12">
                                            <button id="addRecruitersButton" type="button" onClick={this.addRecruiter} class="btn btn-primary">Add</button>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="row">
                    <div class="col-12">
                        <button type="button" class="btn btn-primary addNote" data-toggle="modal" data-target="#addRecruiterModal">
                            Add Recruiter
                    </button>
                    </div>
                </div>

                {/*Recruiters Grid*/}
                <Paper>
                    <Grid
                        rows={rows}
                        columns={columns}
                    >
                        <SearchState
                            value={searchValue}
                            onValueChange={this.changeSearchValue}
                        />

                        <EditingState
                            editingRowIds={editingRowIds}
                            onEditingRowIdsChange={this.changeEditingRowIds}
                            onCommitChanges={this.commitChanges}
                        />
                        <PagingState
                            defaultCurrentPage={0}
                            defaultPageSize={5}
                        />
                        <SortingState
                            sorting={sorting}
                            onSortingChange={this.changeSorting}
                        />
                        <IntegratedFiltering />
                        <IntegratedSorting />
                        <IntegratedPaging />
                        <Table noDataCellComponent={() => <LoadingState columnCount={columns.length} loading={loading} />} />
                        <TableHeaderRow showSortingControls />
                        <TableEditRow />
                        <TableEditColumn
                            showEditCommand
                            showDeleteCommand
                            commandComponent={Command}
                        />
                        <PagingPanel
                            pageSizes={pageSizes}
                        />
                        <Toolbar />
                        <SearchPanel />
                    </Grid>
                </Paper>
            </div>
        );
    }
}

export default RecruitersGrid;
