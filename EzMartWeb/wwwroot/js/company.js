$(document).ready(function () {
    loadTable();
});

function loadTable() {
    $('#tblData').DataTable({
        "ajax": {
            "url": "/admin/company/getall"
        },
        "columns": [
            { "data": "id", "width": "5%" },
            { "data": "name", "width": "20%" },
            { "data": "streetAddress", "width": "20%" },
            { "data": "city", "width": "10%" },
            { "data": "state", "width": "10%" },
            { "data": "phoneNumber", "width": "15%" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                <div class="text-center">
                    <a href="/admin/company/upsert?id=${data}" class="btn btn-success btn-sm text-white" style="cursor:pointer;">
                        <i class="bi bi-pencil-square"></i> Edit
                    </a>
                    <a onClick="Delete('/admin/company/delete/${data}')" class="btn btn-danger btn-sm text-white" style="cursor:pointer;">
                        <i class="bi bi-trash-fill"></i> Delete
                    </a>
                </div>`;
                },
                "width": "20%"
            }
        ]
    });
}

function Delete(url) {
    Swal.fire({
        title: "Are you sure?",
        text: "You won't be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, delete it!"
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'DELETE',
                success: function (data) {
                    if (data.success) {
                        toastr.success(data.message);
                        $('#tblData').DataTable().ajax.reload(); // reload table
                    } else {
                        toastr.error(data.message);
                    }
                }
            });
        }
    });
}