let blazorRef;

window.setBlazorRef = (ref) => {
    blazorRef = ref;
};

window.handleCredentialResponse = async (response) => {
    const token = response.credential;
    const decoded = parseJwt(token);

    const user = {
        email: decoded.email,
        name: decoded.name,
        picture: decoded.picture,
        sub: decoded.sub
    };

    localStorage.setItem("user", JSON.stringify(user));

    if (blazorRef) {
        await blazorRef.invokeMethodAsync("OnGoogleLogin", user.email, user.name, user.picture, user.sub);
    } else {
        console.error("Blazor reference is not set.");
    }
};

function parseJwt(token) {
    const base64Url = token.split('.')[1];
    const base64 = decodeURIComponent(atob(base64Url).split('').map(function (c) {
        return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
    }).join(''));
    return JSON.parse(base64);
}
