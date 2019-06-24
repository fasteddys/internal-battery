var dateTomorrow = new Date();
dateTomorrow.setDate(dateTomorrow.getDate() + 1);

$('.datepicker.begin-tomorrow').datepicker({
    startDate: dateTomorrow,
    autoclose: true
});