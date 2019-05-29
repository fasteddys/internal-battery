const Checkbox = ({className, isChecked, ...props}) => (
    <div className={`react-cc-checkbox ${className !== undefined ? className : ''}`}>
        <div className="hidden-checkbox-wrapper">
            <input type="checkbox" checked={isChecked} {...props} />
        </div>
    </div>
);

export default Checkbox;