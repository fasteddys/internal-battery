import React from 'react';
import { render } from 'react-dom';
import Paper from '@material-ui/core/Paper';
import {
    // State or Local Processing Plugins
} from '@devexpress/dx-react-grid';
import {
    Grid,
    Table,
    TableHeaderRow
} from '@devexpress/dx-react-grid-material-ui';

//class CompaniesGrid extends React.Component {

//    constructor(props) {
//        super(props);
//        //this.state = {
//        //    notifications: props.notifications,
//        //    deviceType: props.deviceType,
//        //    currentNotification: props.notifications[0],
//        //    activeScreen: props.deviceType === "Mobile" ? "list" : "both"
//        //};
//    }
//    render() {
//        return (
//            <div>
//                Bhupesh Vipparla 1
//            </div>
            
//            );
//    }
//}



//import React from 'react';
//import Paper from '@material-ui/core/Paper';
//import {
//    // State or Local Processing Plugins
//} from '@devexpress/dx-react-grid';
//import {
//    Grid,
//    Table,
//    TableHeaderRow
//} from '@devexpress/dx-react-grid-material-ui';

class CompaniesGrid extends React.PureComponent {
    constructor(props) {
        super(props);

        this.state = {
            columns: [
                { name: 'name', title: 'Name' },
                { name: 'sex', title: 'Sex' },
                { name: 'city', title: 'City' },
                { name: 'car', title: 'Car' }
            ],
            rows: [
                { sex: "Female", name: "Sandra", city: "Las Vegas", car: "Audi A4" },
                { sex: "Male", name: "Paul", city: "Paris", car: "Nissan Altima" },
                { sex: "Male", name: "Mark", city: "Paris", car: "Honda Accord" },
                { sex: "Male", name: "Paul", city: "Paris", car: "Nissan Altima" },
                { sex: "Female", name: "Linda", city: "Austin", car: "Toyota Corolla" },
                { sex: "Male", name: "Robert", city: "Las Vegas", car: "Chevrolet Cruze" },
                { sex: "Female", name: "Lisa", city: "London", car: "BMW 750" },
                { sex: "Male", name: "Mark", city: "Chicago", car: "Toyota Corolla" },
                { sex: "Male", name: "Thomas", city: "Rio de Janeiro", car: "Honda Accord" },
                { sex: "Male", name: "Robert", city: "Las Vegas", car: "Honda Civic" },
                { sex: "Female", name: "Betty", city: "Paris", car: "Honda Civic" },
                { sex: "Male", name: "Robert", city: "Los Angeles", car: "Honda Accord" },
                { sex: "Male", name: "William", city: "Los Angeles", car: "Honda Civic" },
                { sex: "Male", name: "Mark", city: "Austin", car: "Nissan Altima" }
            ]
        };
    }
    render() {
        const { rows, columns } = this.state;

        return (
            <Paper>
                <Grid
                    rows={rows}
                    columns={columns}
                >
                    <Table />
                    <TableHeaderRow />
                </Grid>
            </Paper>
            //<div>
            //    Testing
            //</div>
        );
    }
}

export default CompaniesGrid;
