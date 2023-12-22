const { type } = require("jquery");

$(document).ready(function () {
    loadDataTable();
});
function loadDataTable() {
    dataTable = $('#myTable').DataTable({
        "ajax": { url: 'https://localhost:7163/Admin/Product/GetAll' },
        "columns": [
            { data: 'title', "width": "25%"},
            { data: 'isbn', "width": "15%" },
            { data: 'listPrice', "width": "10%" },
            { data: 'author', "width": "15%" },
            { data: 'categoryId', "width": "10%" },
            {
                data: 'id',
                "render": function (data) {
                    return `<div class="w-75 btn-group" role="group">
                    <a href="/product/upsert?id=${data}" class="btn btn-primary mx-2"> <i class="bi bi-pencil-square"></i> Edit<a/>
                     <a href="/product/upsert?id=${data}" class="btn btn-danger mx-2"> <i class="bi bi-trash3"></i> Delete<a/>
                    </div>`
                },
                "width": "25%"
            },
        ]
    })
}
//function Delete(url) {
//    Swal.fire({
//        title: "Are you sure?",
//        text: "You won't be able to revert this!",
//        icon: "warning",
//        showCancelButton: true,
//        confirmButtonColor: "#3085d6",
//        cancelButtonColor: "#d33",
//        confirmButtonText: "Yes, delete it!"
//    }).then((result) => {
//        if (result.isConfirmed) {
//            $.ajax({
//                url: url,
//                type: 'DELETE',
//                success: function (data) {
//                    toastr.success(data.message);
//                }
//            })
//        }
//    });
//}