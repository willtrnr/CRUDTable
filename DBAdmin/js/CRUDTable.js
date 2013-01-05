$("#row-template").template('rowTemplate');

$("form").ajaxForm({
  beforeSubmit: function (arr, form, options) {
    if ($(form).attr("data-validate") && !formValidate(form)) return false;
    $("#update-modal, #delete-modal").modal('hide');
    $("#upload-modal-progressbar").css('width', '0%');
    $("#upload-modal-text").text('0%');
    $("#upload-modal").modal('show');
  },
  error: function () {
    $("#upload-modal").modal('hide');
    $("#error-modal").modal('show');
  },
  dataType: 'json',
  forceSync: true,
  success: function (responseText, statusText, xhr, el) {
    $("#upload-modal").modal('hide');
    $("#success-modal").modal('show');
    setTimeout(function () { $("#success-modal").modal('hide'); }, 2500);
    setTimeout(function () { window.location = window.location; }, 3000);
  },
  uploadProgress: function (event, position, total, percent) {
    $("#upload-modal-progressbar").css('width', percent + '%');
    $("#upload-modal-text").text(percent + '%');
  }
});

$(".update-btn").click(function () {
  var btn = $(this);
  var row = $(btn).parent().parent().find("th, td");
  $("#update-form")[0].reset();
  var skip = 0;
  $("#update-form").find("input, select, textarea").each(function (i, e) {
    if ($(this).attr('data-ignore')) ++skip;
    else if (i < row.length && $(this).attr('type') !== 'file') {
      $(this).val($(row[i - skip]).text());
    }
  });
  $("#update-modal").modal('show');
});

$(".delete-btn").click(function () {
  var btn = $(this);
  var row = $(btn).parent().parent().find("th, td");
  var col = $("#delete-modal-table tr");
  $("#delete-modal-table td").remove();
  for (var i in row) {
    if (i < col.length) {
      $(document.createElement('td')).html($(row[i]).html()).appendTo($(col[i]));
    }
  }
  $("#delete-form input").each(function (i, e) {
    $(this).val($(btn).attr('data-' + $(this).attr('name')));
  });
  $("#delete-modal").modal('show');
});

$("#success-modal-btn").click(function () {
  window.location = window.location;
});
