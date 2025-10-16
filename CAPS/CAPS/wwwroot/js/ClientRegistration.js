$(document).ready(function () {
    $('#registerBtn').on('click', function (e) {
        e.preventDefault();

        // Validate form before showing modal
        var form = $('#clientRegistrationForm');
        var isValid = true;

        // Check required fields
        form.find('input[required], select[required]').each(function () {
            if (!$(this).val()) {
                isValid = false;
                $(this).addClass('is-invalid');
            } else {
                $(this).removeClass('is-invalid');
            }
        });

        // If form is invalid, show alert and stop submission
        if (!isValid) {
            alert('Please fill in all required fields before registering.');
            return;
        }

        // Submit the form if valid
        form.submit();
    });
});