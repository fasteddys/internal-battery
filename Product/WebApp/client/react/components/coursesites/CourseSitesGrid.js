import React from 'react';
import Paper from '@material-ui/core/Paper';
import CircularProgress from '@material-ui/core/CircularProgress';

import IconButton from '@material-ui/core/IconButton';
import SyncIcon from '@material-ui/icons/Sync';
import CrawlIcon from '@material-ui/icons/Update';

import {
    PagingState,
    IntegratedPaging,
    IntegratedFiltering,
    SortingState,
    IntegratedSorting
} from '@devexpress/dx-react-grid';
import {
    Grid,
    Table,
    Toolbar,
    TableHeaderRow,
    PagingPanel,
} from '@devexpress/dx-react-grid-material-ui';

const CrawlButton = ({ onExecute }) => (
    <IconButton onClick={onExecute} title="Crawl">
        <CrawlIcon />
    </IconButton>
);

const SyncButton = ({ onExecute }) => (
    <IconButton color="secondary" onClick={onExecute} title="Sync">
        <SyncIcon />
    </IconButton>
);

const LoadingState = ({ loading, columnCount }) => (
    <td colSpan={columnCount} style={{ textAlign: 'center', verticalAlign: 'middle' }}>
        <big>
            {loading ? (<CircularProgress size={28} />) : (<span>No course sites</span>)}
        </big>
    </td>
)

class CourseSitesGrid extends React.PureComponent {

    constructor(props) {
        super(props);

        this.state = {
            columns: [
                { name: 'Name', title: 'Name', dataType: 'string' },
                { name: 'LastCrawl', title: 'Last Crawl', dataType: 'datetime' },
                { name: 'LastSync', title: 'Last Sync', dataType: 'datetime' },
                { name: 'SyncCount', title: 'Sync Count', dataType: 'int' },
                { name: 'CreateCount', title: 'Create Count', dataType: 'int' },
                { name: 'UpdateCount', title: 'Update Count', dataType: 'int' },
                { name: 'DeleteCount', title: 'Delete Count', dataType: 'int' },
                { name: 'ErrorCount', title: 'Error Count', dataType: 'int' }
            ],
            rows: [],
            pageSizes: [5, 10, 15, 0],
            sorting: [],
            rowChanges: [],
            loading: true
        };

        this.changeSorting = sorting => this.setState({ sorting });
    }

    componentDidMount() {
        this.getCourseSites();
    }

    getCourseSites() {
        CareerCircleAPI.getCourseSites()
            .then((res) => {
                this.setState({ rows: this.buildRows(res.data) })
            }).catch(() => ToastService.error('An error occured while loading the course sites.'))
            .finally(() => {
                this.setState({ loading: false });
            })
    }

    buildRows(data) {
        let rowData = [];
        data.forEach(cs => rowData.push(
            {
                CourseSiteGuid: cs.courseSiteGuid,
                Name: cs.name,
                Uri: cs.uri,
                LastCrawl: cs.lastCrawl,
                LastSync: cs.lastSync,
                CreateCount: cs.createCount,
                UpdateCount: cs.updateCount,
                DeleteCount: cs.deleteCount,
                ErrorCount: cs.errorCount,
                SyncCount: cs.syncCount
            }));

        return rowData;
    }
    
    render() {
        const { rows, columns, actionColumns, pageSizes, sorting, loading} = this.state;

        return (
            <div>              
                <Paper>
                    <Grid rows={rows} columns={columns}>
                        <PagingState defaultCurrentPage={0} defaultPageSize={5} />
                        <SortingState sorting={sorting} onSortingChange={this.changeSorting} />
                        <IntegratedFiltering />
                        <IntegratedSorting />
                        <IntegratedPaging />
                        <Table noDataCellComponent={() => <LoadingState columnCount={columns.length} loading={loading} />} />
                        <TableHeaderRow showSortingControls />
                        <PagingPanel pageSizes={pageSizes} />
                        <Toolbar />
                    </Grid>
                </Paper>
            </div>
        );
    }
}

export default CourseSitesGrid;