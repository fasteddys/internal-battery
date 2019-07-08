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
                { name: 'FirstName', title: 'First Name', dataType:'string' },
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
        };

        this.changeSearchValue = value => this.setState({ searchValue: value });
        this.changeSorting = sorting => this.setState({ sorting });
        this.changeEditingRowIds = this.changeEditingRowIds.bind(this);
        this.commitChanges = this.commitChanges.bind(this);
        this.invalidEditedRowIds = [];
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

    render() {
        const { rows, columns, pageSizes, searchValue, sorting, editingRowIds, loading } = this.state;

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
        );
    }
}

export default RecruitersGrid;
