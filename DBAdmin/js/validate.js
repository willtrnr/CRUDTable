Date.prototype.getDaysInMonth = function () {
  var start = new Date(this.getYear(), this.getMonth(), 1);
  var end = new Date(this.getYear(), this.getMonth() + 1, 1);
  return Math.floor((end - start) / (1000 * 60 * 60 * 24));
}

function validateNotNull(dirty) {
  return (dirty != null && dirty !== "" && !/^\s+$/.test(dirty));
}

function validateNum(dirty) {
  return (/^\d+$/.test(dirty));
}

function validateDec(dirty) {
  return (/^[\d\.](?:\d*(?:\.\d+)?)?$/.test(dirty));
}

function validateAlpha(dirty) {
  return (/^[A-Za-z]+$/.test(dirty));
}

function validateAlphaSp(dirty) {
  return (/^[A-Za-z\-\.,' ]+$/.test(dirty));
}

function validateAlNum(dirty) {
  return (/^[A-Za-z\d]+$/.test(dirty));
}

function validateAlNumSp(dirty) {
  return (/^[A-Za-z\d\-\.,' ]+$/.test(dirty));
}

function validateDate(dirty) {
  return (/^[\d\/\-]+$/.test(dirty));
}

function setValidateError(input, msg, twitterBootstrap) {
  var parent = $(input).parent();
  if (twitterBootstrap) {
    var limit = 6;
    var controls = parent;
    // TODO: God, I'm drunk, what the hell is this
    while (!$(controls).hasClass('controls') && --limit >= 0) {
      controls = $(controls).parent();
    }
    if ($(controls).hasClass('controls')) {
      parent = controls;
    }

    limit = 6;
    var control_group = parent;
    // TODO: #whatwasithinking
    while (!$(control_group).hasClass('control-group') && --limit >= 0) {
      control_group = $(control_group).parent();
    }
    if ($(control_group).hasClass('control-group')) {
      if (!$(control_group).hasClass('error')) {
        $(control_group).addClass('error');
      }
    }
  }
  var help_inline = $(parent).find('.help-inline');
  if (help_inline.length) {
    $(help_inline[0]).html(msg);
  } else {
    $(document.createElement('div')).addClass('help-inline').html(msg).appendTo($(parent));
  }
}

function clearValidateError(input, twitterBootstrap) {
  var parent = $(input).parent();
  if (twitterBootstrap) {
    var limit = 6;
    var controls = parent;
    // TODO: This is ugly as hell
    while (!$(controls).hasClass('controls') && --limit >= 0) {
      controls = $(controls).parent();
    }
    if ($(controls).hasClass('controls')) {
      parent = controls;
    }

    limit = 6;
    var control_group = parent;
    // TODO: My eyes are bleeding
    while (!$(control_group).hasClass('control-group') && --limit >= 0) {
      control_group = $(control_group).parent();
    }
    if ($(control_group).hasClass('control-group')) {
      $(control_group).removeClass('error');
    }
  }
  $(parent).find('.help-inline').remove();
}

function inputValidate(input, twitterBootstrap, override_val) {
  var field_val = $(input).val();
  var val = override_val || field_val;
  console.log("Validating '" + val + "'");
  var input_settings = $(input).attr('data-validate').toLowerCase().split(/\s+/);
  for (var i = 0; i < input_settings.length; ++i) {
    var setting = input_settings[i];
    if (setting == "notnull" && !override_val) {
      if (!validateNotNull(val)) {
        setValidateError(input, $(input).attr('data-validate-notnull') || "Field is required!", twitterBootstrap);
        return false;
      }
    } else if (setting == "num") {
      if (!validateNum(val)) {
        setValidateError(input, $(input).attr('data-validate-num') || "Only digits are allowed!", twitterBootstrap);
        return false;
      }
    } else if (setting == "dec") {
      if (!validateDec(val)) {
        setValidateError(input, $(input).attr('data-validate-dec') || "Only digits and . are allowed!", twitterBootstrap);
        return false;
      }
    } else if (setting == "alpha") {
      if (!validateAlpha(val)) {
        setValidateError(input, $(input).attr('data-validate-alpha') || "Only alphabetic characters are allows!", twitterBootstrap);
        return false;
      }
    } else if (setting == "alphasp") {
      if (!validateAlphaSp(val)) {
        setValidateError(input, $(input).attr('data-validate-alphasp') || "Your input contains invalid characters!", twitterBootstrap);
        return false;
      }
    } else if (setting == "alnum") {
      if (!validateAlNum(val)) {
        setValidateError(input, $(input).attr('data-validate-alnum') || "Only alpha-numeric characters are allowed!", twitterBootstrap);
        return false;
      }
    } else if (setting == "alnumsp") {
      if (!validateAlNumSp(val)) {
        setValidateError(input, $(input).attr('data-validate-alnumsp') || "Your input contains invalid characters!", twitterBootstrap);
        return false;
      }
    } else if (setting == "date") {
      if (!validateDate(val)) {
        setValidateError(input, $(input).attr('data-validate-date') || "Enter your date in the format yyyy/mm/dd!", twitterBootstrap);
        return false;
      }
    } else if (/^maxlen=\d+,?(?:[^\s]+)?$/.test(setting)) {
      var maxlen = setting.match(/^maxlen=(\d+),?([^\s]+)?$/);
      if (maxlen) {
        var option = maxlen[2];
        maxlen = maxlen[1];
        if (field_val.length + 1 == maxlen && option == "next") {
          clearValidateError(input, twitterBootstrap);
          var next = $(input).next('input');
          $(next).focus();
          $(next).caret(0, $(next).val().length);
          return true;
        } else if (field_val.length > maxlen - ((override_val) ? 1 : 0) && $(input).caret().start == $(input).caret().end) {
          setValidateError(input, $(input).attr('data-validate-maxlen') || "A maximum of " + maxlen + " characters is allowed!", twitterBootstrap);
          return false;
        }
      }
    }
  }
  clearValidateError(input, twitterBootstrap);
  return true;
}

function formValidate(form) {
  var valid = true;
  var form_settings = $(form).attr('data-validate').toLowerCase().split(/\s+/);
  $(form).find("input, select").each(function (i, e) {
    if ($(e).attr('data-validate')) {
      if (!inputValidate(e, (form_settings.indexOf('bootstrap') !== -1))) {
        valid = false;
      }
    }
  });
  return valid;
}

function formValidateHandler(e) {
  var valid = formValidate(this);
  if (!valid) {
    e.preventDefault();
  }
  return valid;
}

function inputValidateHandler(e) {
  inputValidate(this, false);
}

function bootstrapInputValidateHandler(e) {
  inputValidate(this, true);
}

function keypressInputValidateHandler(e) {
  var valid = inputValidate(this, false, String.fromCharCode(e.which));
  if (!valid) {
    e.preventDefault();
  }
  return valid;
}

function keypressBootstrapInputValidateHandler(e) {
  var valid = inputValidate(this, true, String.fromCharCode(e.which));
  if (!valid) {
    e.preventDefault();
  }
  return valid;
}

window.onload = function () {
  $("form").each(function (index, form) {
    if ($(form).attr('data-validate')) {
      var form_settings = $(form).attr('data-validate').toLowerCase().split(/\s+/);
      if (form_settings.indexOf("form") !== -1) {
        $(form).submit(formValidateHandler);
        $(form).find("input, select").each(function (i, input) {
          if ($(input).attr('data-validate')) {
            var input_settings = $(input).attr('data-validate').split(/\s+/);
            if (form_settings.indexOf('bootstrap') !== -1) {
              $(input).change(bootstrapInputValidateHandler);
              if (input_settings.indexOf('onkey') !== -1) {
                $(input).keypress(keypressBootstrapInputValidateHandler);
              }
            } else {
              $(input).change(inputValidateHandler);
              if (input_settings.indexOf('onkey') !== -1) {
                $(input).keypress(keypressInputValidateHandler);
              }
            }
          }
        });
      }
    }
  });
};
