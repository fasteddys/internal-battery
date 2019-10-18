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

const AddButton = ({ onExecute }) => (
    <div class="row">
        <div class="col-12">
            <button type="button" class="btn btn-primary addNote text-right" data-toggle="modal" data-target="#addRecruiterModal" >
                Add
                    </button>
        </div>
    </div>
);

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
    add: AddButton,
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

const BooleanFormatter = ({ value }) => <Chip label={value ? 'Yes' : 'No'} />;

const BooleanEditor = ({ value, onValueChange }) => (
    <Select
        input={<Input />}
        value={value ? 'Yes' : 'No'}
        onChange={event => onValueChange(event.target.value === 'Yes')}
        style={{ width: '100%' }}
    >
        <MenuItem value="Yes">
            Yes
    </MenuItem>
        <MenuItem value="No">
            No
    </MenuItem>
    </Select>
);

const BooleanTypeProvider = props => (
    <DataTypeProvider
        formatterComponent={BooleanFormatter}
        editorComponent={BooleanEditor}
        {...props}
    />
);

const StringEditor = ({ value, onValueChange, column: { value: defaultValue, mandatory } }) => (
    <TextField
        error={mandatory && (!defaultValue && !value)}
        defaultValue={value || defaultValue}
        onChange={e => onValueChange(e.target.value)}
        required
        label="Required"
    />
);

const StringTypeProvider = props => (
    <DataTypeProvider
        editorComponent={StringEditor}
        {...props}
    />
);

const LabelEditor = ({ value, onValueChange, column: { value: defaultValue, mandatory } }) => (
    <TextField
        defaultValue={value || defaultValue}
        disabled
    />
);

const LabelTypeProvider = props => (
    <DataTypeProvider
        editorComponent={LabelEditor}
        {...props}
    />
);

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
                { name: 'IsRecruiter', title: 'Recruiter Talent Access' },
                { name: 'CompanyName', title: 'Company Name', dataType: 'string' },
            ],
            booleanColumns: ['IsRecruiter'],
            labelColumns: ['Email', 'CompanyName'],
            stringColumns: ['FirstName', 'LastName'],
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
                isInAuth0RecruiterGroupRecruiter: true,
                subscriberGuid: '',
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
        this.changeFirstName = this.changeFirstName.bind(this);
        this.changeLastName = this.changeLastName.bind(this);
        this.changePhone = this.changePhone.bind(this);
        this.clearRecruiterDetails = this.clearRecruiterDetails.bind(this);
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
                Phone: x.phoneNumber,
                IsRecruiter: x.is,
                CompanyName: x.company.companyName,
                RecruiterGuid: x.recruiterGuid,
                SubscriberGuid: x.subscriber.subscriberGuid
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

    validateEditChanges(rowChanged, editedRecruiter) {
        let status = true;
        if (editedRecruiter != undefined && editedRecruiter != null) {

            //validate firstName
            if (editedRecruiter.FirstName != undefined && editedRecruiter.FirstName != null) {
                if (editedRecruiter.FirstName != '')
                    status = true;
                else {
                    this.invalidEditedRowIds = this.invalidEditedRowIds.concat(parseInt(rowChanged));
                    status = false;
                    return status;
                }
            }

            //validate lastName
            if (editedRecruiter.LastName != undefined && editedRecruiter.LastName != null) {
                if (editedRecruiter.LastName != '')
                    status = true;
                else {
                    this.invalidEditedRowIds = this.invalidEditedRowIds.concat(parseInt(rowChanged));
                    status = false;
                    return status;
                }
            }

            //validate Phone Number
            if (editedRecruiter.Phone != undefined && editedRecruiter.Phone != null) {
                var phoneno = /^[2-9]{1}\d{9}$/;
                if (editedRecruiter.Phone.match(phoneno)) {
                    status = true;
                }
                else {
                    this.invalidEditedRowIds = this.invalidEditedRowIds.concat(parseInt(rowChanged));
                    ToastService.error('Invalid Phone Number.')
                    status = false;
                    return status;
                }
            }

            return status;
        }
        else {
            this.invalidEditedRowIds = this.invalidEditedRowIds.concat(parseInt(rowChanged));
            status = false;
            return status;
        }
    }

    validateAddChanges(addedRecruiter) {
        let status = true;
        if (addedRecruiter != undefined && addedRecruiter != null) {

            //validate firstName
            if (addedRecruiter.email != undefined && addedRecruiter.email != null) {
                if (addedRecruiter.email != '') {

                }
                else {
                    ToastService.error('Please Search by Email.');
                    status = false;
                    return status
                }
            }
            else {
                ToastService.error('Please Enter First Name.');
                status = false;
            }

            //validate firstName
            if (addedRecruiter.firstName != undefined && addedRecruiter.firstName != null) {
                if (addedRecruiter.firstName != '') {

                }
                else {
                    ToastService.error('Please Enter First Name.');
                    status = false;
                }
            }
            else {
                ToastService.error('Please Enter First Name.');
                status = false;
            }

            //validate lastName
            if (addedRecruiter.lastName != undefined && addedRecruiter.lastName != null) {
                if (addedRecruiter.lastName != '')
                {

                }
                else {
                    ToastService.error('Please Enter Last Name.');
                    status = false;
                }
            }
            else {
                ToastService.error('Please Enter Last Name.');
                status = false;
            }

            //validate Phone Number
            if (addedRecruiter.phone != undefined && addedRecruiter.phone != null) {
                var phoneno = /^[2-9]{1}\d{9}$/;
                if (addedRecruiter.phone.match(phoneno)) {

                }
                else {
                    ToastService.error('Invalid Phone Number.')
                    status = false;
                }
            }

            return status;
        }
        else {
            status = false;
            return status;
        }
    }

    commitChanges({ changed, deleted }) {
        let { rows } = this.state;
        if (changed) {
            var changedRowId = Object.keys(changed)[0];
            var unchangedRowData = rows[changedRowId];
            if (this.validateEditChanges(changedRowId, changed[changedRowId])) {
                var recruiter = {
                    RecruiterGuid: unchangedRowData.RecruiterGuid,
                    FirstName: changed[changedRowId].FirstName == undefined ? unchangedRowData.FirstName : changed[changedRowId].FirstName,
                    LastName: changed[changedRowId].LastName == undefined ? unchangedRowData.LastName : changed[changedRowId].LastName,
                    PhoneNumber: changed[changedRowId].Phone == undefined ? unchangedRowData.Phone : changed[changedRowId].Phone,
                    IsInAuth0RecruiterGroupRecruiter: changed[changedRowId].IsRecruiter == undefined ? unchangedRowData.IsRecruiter : changed[changedRowId].IsRecruiter,
                    SubscriberGuid: unchangedRowData.SubscriberGuid,
                }
                CareerCircleAPI.editRecruiter(recruiter)
                    .then((res) => {
                        this.setState({ loading: true, rows: [] });
                        ToastService.success('Recruiter updated successfully.');
                        this.getRecruiters();
                    })
                    .catch(() => ToastService.error('An error occured while updating the recruiter.'))

            }

        }
        if (deleted) {
            var changedRowId = deleted[0];
            var unchangedRowData = rows[changedRowId];

            var recruiter = {
                RecruiterGuid: unchangedRowData.RecruiterGuid,
                SubscriberGuid: unchangedRowData.SubscriberGuid,
            }

            CareerCircleAPI.deleteRecruiter(recruiter)
                .then((res) => {
                    this.setState({ loading: true, rows: [] });
                    ToastService.success('Recruiter deleted successfully.');
                    this.getRecruiters();
                })
                .catch(() => ToastService.error('An error occured while deleting the recruiter.'))
        }
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
                        if (res.data != undefined && res.data != null && res.data != '') {
                            this.setState({ recruiter: this.buildRecruiter(res.data) })
                        }
                    }
                    else {
                        ToastService.error('Invalid Email Address')
                    }
                })
                .catch(() => ToastService.error('An error occured while loading the recruiter.'))
        }
        else {
            ToastService.error('Invalid Email Address')
        }


    }

    buildRecruiter(data) {
        let recruiterData = {
            firstName: data.firstName == null ? '' : data.firstName,
            lastName: data.lastName == null ? '' : data.lastName,
            email: data.email,
            phone: data.phoneNumber == null ? '' : data.phoneNumber,
            subscriberGuid: data.subscriberGuid,
            isInAuth0RecruiterGroupRecruiter: this.state.recruiter.isInAuth0RecruiterGroupRecruiter
        };

        return recruiterData;
    }

    changePermissions() {
        this.setState({
            recruiter: {
                firstName: this.state.recruiter.firstName,
                lastName: this.state.recruiter.lastName,
                email: this.state.recruiter.email,
                phone: this.state.recruiter.phone,
                subscriberGuid: this.state.recruiter.subscriberGuid,
                isInAuth0RecruiterGroupRecruiter: !this.state.recruiter.isInAuth0RecruiterGroupRecruiter
            }
        })
    }

    changeFirstName(value) {
        this.setState({
            recruiter: {
                firstName: value,
                lastName: this.state.recruiter.lastName,
                email: this.state.recruiter.email,
                phone: this.state.recruiter.phone,
                subscriberGuid: this.state.recruiter.subscriberGuid,
                isInAuth0RecruiterGroupRecruiter: this.state.recruiter.isInAuth0RecruiterGroupRecruiter
            }
        })
    }

    changeLastName(value) {
        this.setState({
            recruiter: {
                firstName: this.state.recruiter.firstName,
                lastName: value,
                email: this.state.recruiter.email,
                phone: this.state.recruiter.phone,
                subscriberGuid: this.state.recruiter.subscriberGuid,
                isInAuth0RecruiterGroupRecruiter: this.state.recruiter.isInAuth0RecruiterGroupRecruiter
            }
        })
    }

    changePhone(value) {
        this.setState({
            recruiter: {
                firstName: this.state.recruiter.firstName,
                lastName: this.state.recruiter.lastName,
                email: this.state.recruiter.email,
                phone: value,
                subscriberGuid: this.state.recruiter.subscriberGuid,
                isInAuth0RecruiterGroupRecruiter: this.state.recruiter.isInAuth0RecruiterGroupRecruiter
            }
        })
    }

    addRecruiter(e) {
        e.preventDefault();
        if (this.validateAddChanges(this.state.recruiter)) {
            CareerCircleAPI.addRecruiter((this.state.recruiter))
                .then((res) => {
                    if (res.statusText == 'OK') {
                        if (res.data == 'Exist') {
                            ToastService.info('Recruiter already exit.');
                        }
                        else if (res.data == 'Invalid') {
                            ToastService.error('Invalid data.');
                        }
                        else {
                            this.setState({ loading: true, rows: [] });
                            ToastService.success('Recruiter added successfully.');
                            this.getRecruiters();
                        }
                    }

                })
                .catch((err) => ToastService.error('An error occured while adding the recruiter.'))
        }
    }

    clearRecruiterDetails() {
        this.setState({
            recruiter: {
                firstName: '',
                lastName: '',
                email: '',
                phone: '',
                subscriberGuid: '',
                isInAuth0RecruiterGroupRecruiter: this.state.recruiter.isInAuth0RecruiterGroupRecruiter
            }
        })

        this.searchRecruiter.value=''

    }

    render() {
        const { rows, columns, booleanColumns, labelColumns, stringColumns, pageSizes, searchValue, sorting, editingRowIds, loading, recruiter, isRecruiter } = this.state;

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
                                        <div class="col-8">
                                            <input id="searchRecruitersBox" type="text" placeholder="   Search by Email" ref={(value) => this.searchRecruiter = value} />
                                        </div>
                                        <div class="col-4">
                                            <button id="searchRecruitersButton" type="button" onClick={this.searchRecruiters} class="btn btn-primary">Search</button>
                                        </div>
                                    </div>
                                    <br/>
                                    <div class="row">
                                        <div class="col-12">
                                            <div class="form-group">
                                                <div class="row">
                                                    <div class="col-4">
                                                        <label for="firstName">First Name :</label>
                                                    </div>
                                                    <div class="col-8">
                                                        <input type="text" id="firstName" placeholder="John" value={recruiter.firstName} onChange={e => this.changeFirstName(e.target.value)} />
                                                    </div>
                                                </div>
                                                <div class="row">
                                                    <div class="col-4">
                                                        <label for="lastName">Last Name :</label>
                                                    </div>
                                                    <div class="col-8">
                                                        <input type="text" id="lastName" placeholder="Smith" value={recruiter.lastName} onChange={e => this.changeLastName(e.target.value)} />
                                                    </div>
                                                </div>
                                                <div class="row">
                                                    <div class="col-4">
                                                        <label for="email">Email :</label>
                                                    </div>
                                                    <div class="col-8">
                                                        <label>{recruiter.email}</label>
                                                    </div>
                                                </div>
                                                <div class="row">
                                                    <div class="col-4">
                                                        <label for="phone">Phone :</label>
                                                    </div>
                                                    <div class="col-8">
                                                        <input type="text" id="phone" placeholder="1234567890" value={recruiter.phone} onChange={e => this.changePhone(e.target.value)} />
                                                    </div>
                                                </div>
                                                <div class="row">
                                                    <div class="col-4">
                                                        <label for="isRecruiter">Is Recruiter</label>
                                                    </div>
                                                    <div class="col-4">
                                                        <input type="checkbox" id="isRecruiter" checked={recruiter.isInAuth0RecruiterGroupRecruiter} onChange={this.changePermissions} />
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                                <div class="modal-footer">
                                    <button type="button" class="btn btn-secondary" data-dismiss="modal">Close</button>
                                    <button id="addRecruitersButton" type="button" onClick={this.addRecruiter} class="btn btn-primary">Add</button>
                                </div>

                            </div>
                        </div>
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

                        <BooleanTypeProvider
                            for={booleanColumns}
                        />

                        <LabelTypeProvider
                            for={labelColumns}
                        />

                        <StringTypeProvider
                            for={stringColumns}
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
                            showAddCommand
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
