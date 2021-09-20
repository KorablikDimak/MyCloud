async function sendJsonMessage (uri, method, message = null) {
    let init;
    if (message == null) {
        init = {
            method: method
        }
    }
    else {
        init = {
            method: method,
            headers: {
                'Content-Type': 'application/json;charset=utf-8'
            },
            body: JSON.stringify(message)
        }
    }
    return await fetch(uri, init);
}