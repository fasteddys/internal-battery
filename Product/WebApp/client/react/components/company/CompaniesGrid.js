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

import { withStyles } from '@material-ui/core/styles';
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
    <div style={{ textAlign: 'center' }}>
        <button
            type="button"
            class="btn btn-primary"
            onClick={onExecute}
            title="Create new row"
        >
            Add
    </button>
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

const LoadingState = ({ loading, columnCount }) => (
    <td colSpan={columnCount} style={{ textAlign: 'center', verticalAlign: 'middle' }}>
        <big>
            {loading ? ( <CircularProgress size={28} /> ) : (<span>No data</span>)}
        </big>
    </td>
)

class CompaniesGrid extends React.PureComponent {
    constructor(props) {
        super(props);

        this.state = {
            columns: [
                { name: 'CompanyName', title: 'Company Name' },
                { name: 'IsHiringAgency', title: 'Is Hiring Agency', dataType: 'boolean' },
                { name: 'IsJobPoster', title: 'Is Job Poster', dataType: 'boolean' }
            ],
            booleanColumns: ['IsHiringAgency', 'IsJobPoster'],
            stringColumns: ['CompanyName'],
            rows:[],
            pageSizes: [5, 10, 15, 0],
            searchValue: '',
            editingRowIds: [],
            sorting: [],
            addedRows: [],
            rowChanges: [],
            loading: true,
        };

        this.changeSearchValue = value => this.setState({ searchValue: value });
        this.changeSorting = sorting => this.setState({ sorting });
        this.changeEditingRowIds = this.changeEditingRowIds.bind(this);
        this.commitChanges = this.commitChanges.bind(this);
        this.addedRowsChange = this.addedRowsChange.bind(this);
        this.invalidAddedRows = [];
        this.invalidEditedRowIds = [];
    }

    componentDidMount() {
        this.getCompanies();
    }

    getCompanies() {
        CareerCircleAPI.getCompanies()
            .then((res) => {
                this.setState({ rows: this.buildRows(res.data) })
            }).catch(() => ToastService.error('An error occured while loading the companies.'))
            .finally(() => {
                this.setState({ loading: false });
            })
    }

    

    buildRows(data) {
        let rowData = [];
        data.forEach(x => rowData.push(
            {
                CompanyName: x.companyName,
                IsHiringAgency: x.isHiringAgency,
                IsJobPoster: x.isJobPoster,
                CompanyGuid:x.companyGuid
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

    validateAddChanges(data) {
        const { rows } = this.state;
        if (data != undefined && data != null && data.CompanyName != undefined && data.CompanyName != null && data.CompanyName != '') {
            var existingCompanies = rows.filter(x => x.CompanyName.toLowerCase() == data.CompanyName.toLowerCase());
            if (existingCompanies.length > 0) {
                this.invalidAddedRows = this.invalidAddedRows.concat(data); 
                ToastService.error('Company already exist.');
                return false;
            }
            return true;         
        }
        else {
            this.invalidAddedRows = this.invalidAddedRows.concat(data); 
            return false;
        }
    }

    validateEditChanges(rowChanged, editedCompany) {
        const { rows } = this.state;
        if (editedCompany != undefined && editedCompany != null) {

            if (editedCompany.CompanyName == undefined && editedCompany.CompanyName == null) {
                return true;
            } else if (editedCompany.CompanyName != '') {
                var existingCompanies = rows.filter(x => x.CompanyName.toLowerCase() == editedCompany.CompanyName.toLowerCase());
                if (existingCompanies.length > 0) {
                    this.invalidEditedRowIds = this.invalidEditedRowIds.concat(parseInt(rowChanged));
                    ToastService.error('Company already exist.');
                    return false;
                }
                return true;
            }
            
            else {
                    this.invalidEditedRowIds = this.invalidEditedRowIds.concat(parseInt(rowChanged));
                    return false;
            }
            return true;
        }
        else {
            this.invalidEditedRowIds = this.invalidEditedRowIds.concat(parseInt(rowChanged));
            return false;
        }
    }

    addedRowsChange(addedRows) {
        // Update state according to invalid added rows
        if (this.invalidAddedRows.length) {
            this.setState({ addedRows: [...this.invalidAddedRows] });
            this.invalidAddedRows.length = 0;
        } else {
            this.setState({ addedRows });
        }
    }

    commitChanges({ added, changed, deleted } ) {
        let { rows } = this.state;
        if (added) {
            if (this.validateAddChanges(added[0])) {
                //add new company
                var newCompany = {
                    CompanyName: added[0].CompanyName,
                    IsHiringAgency: added[0].IsHiringAgency == undefined ? false : added[0].IsHiringAgency,
                    IsJobPoster: added[0].IsJobPoster == undefined ? false : added[0].IsJobPoster
                }
                CareerCircleAPI.addCompany(newCompany)
                    .then((res) => {
                        this.setState({ loading: true, rows: [] });
                        ToastService.success('Company added successfully.');
                        this.getCompanies();
                    })
                    .catch(() => ToastService.error('An error occured while adding a new company.'))
            }
        }
        if (changed) {
            var changedRowId = Object.keys(changed)[0];
            var unchangedRowData = rows[changedRowId];
            if (this.validateEditChanges(changedRowId, changed[changedRowId])) {
                var company = {
                    CompanyGuid: unchangedRowData.CompanyGuid,
                    CompanyName: changed[changedRowId].CompanyName == undefined ? unchangedRowData.CompanyName : changed[changedRowId].CompanyName,
                    IsHiringAgency: changed[changedRowId].IsHiringAgency == undefined ? unchangedRowData.IsHiringAgency : changed[changedRowId].IsHiringAgency,
                    IsJobPoster: changed[changedRowId].IsJobPoster == undefined ? unchangedRowData.IsJobPoster : changed[changedRowId].IsJobPoster
                }
                    CareerCircleAPI.editCompany(company)
                        .then((res) => {
                            this.setState({ loading: true, rows: [] });
                            ToastService.success('Company updated successfully.');
                            this.getCompanies();
                        })
                        .catch(() => ToastService.error('An error occured while updating the company.'))

            }
                
        }
        if (deleted) {
            var changedRowId = deleted[0];
            var unchangedRowData = rows[changedRowId];

            var companyGuid = unchangedRowData.CompanyGuid;

            CareerCircleAPI.deleteCompany(companyGuid)
                .then((res) => {
                    this.setState({ loading: true, rows: [] });
                    ToastService.success('Company deleted successfully.');
                    this.getCompanies();
                })
                .catch(() => ToastService.error('An error occured while deleting the company.'))
        }
    }

    render() {
        const { rows, columns, stringColumns, booleanColumns, pageSizes, searchValue, sorting, editingRowIds, addedRows, loading } = this.state;

        return (
            <Paper>
                <Grid
                    rows={rows}
                    columns={columns}
                >
                    <SearchState
                        value={searchValue}
                        onValueChange={this.changeSearchValue}
                    /> 
                    <StringTypeProvider for={stringColumns}/>
                    <BooleanTypeProvider
                        for={booleanColumns}
                    />
                    <EditingState
                        editingRowIds={editingRowIds}
                        onEditingRowIdsChange={this.changeEditingRowIds}
                        onCommitChanges={this.commitChanges}
                        addedRows={addedRows}
                        onAddedRowsChange={this.addedRowsChange}
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
                    <TableHeaderRow showSortingControls/>
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
        );
    }
}

export default CompaniesGrid;
