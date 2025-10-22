window.blazorCulture = {
    get: function () {
        return localStorage.getItem('BlazorCulture');
    },
    set: function (value) {
        localStorage.setItem('BlazorCulture', value);
        location.reload(); 
    }
};