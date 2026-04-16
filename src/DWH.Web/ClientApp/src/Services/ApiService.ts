import axios, {type AxiosInstance} from "axios";

export const API_BASE = "http://localhost:5000"

export class ApiService {
    api: AxiosInstance;

    constructor(postfix: string) {
        this.api = axios.create({
            baseURL: API_BASE + "/api/" + postfix,
            timeout: 200000,
            headers: {Accept: "application/json"}
        });
    }
}
