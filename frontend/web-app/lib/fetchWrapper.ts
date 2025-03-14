import { auth } from "@/auth"

const baseUrl = process.env["services__gatewayservice__http__0"]

const handleResponse = async (response: Response) => {
    const text = await response.text()
    const data = text && JSON.parse(text)
    
    if (response.ok)
    {
        return data || response.statusText
    }
    else {
        const error = {
            status: response.status,
            message: response.statusText
        }
        return {error}
    }
}

const getCommonHeaders = async () => {
    const session = await auth()
    const headers = {
        "Content-type": "application/json",
    } as HeadersInit & {Authorization?: string}
    if(session?.accessToken) headers.Authorization = "Bearer " + session.accessToken
    
    return headers
}

const get = async (url: string, init?: RequestInit) => {
    const requestOptions = {
        method: 'GET',
        headers: await getCommonHeaders(),
        ...init
    }
    
    const response = await fetch(baseUrl + url, requestOptions);
    
    return handleResponse(response);
}

const post = async (url: string, body: {}, init?: RequestInit) => {
    const requestOptions = {
        method: 'POST',
        headers: await getCommonHeaders(),
        body: JSON.stringify(body),
        ...init
    }

    const response = await fetch(baseUrl + url, requestOptions);

    return handleResponse(response);
}

const put = async (url: string, body: {}, init?: RequestInit) => {
    const requestOptions = {
        method: 'PUT',
        headers: await getCommonHeaders(),
        body: JSON.stringify(body),
        ...init
    }

    const response = await fetch(baseUrl + url, requestOptions);

    return handleResponse(response);
}

const del = async (url: string, init?: RequestInit) => {
    const requestOptions = {
        method: 'DELETE',
        headers: await getCommonHeaders(),
        ...init
    }

    const response = await fetch(baseUrl + url, requestOptions);

    return handleResponse(response);
}

export const fetchWrapper = {
    get, post, put, del
}