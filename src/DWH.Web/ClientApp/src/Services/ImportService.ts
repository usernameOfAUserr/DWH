import { ApiService } from "./ApiService";
import type {ImportStatus} from "@/Models/ImportStatus.ts";

const importApi = new ApiService("import");

export const ImportService = {
    async startImport(): Promise<void> {
        await importApi.api.post(
            "/start",
            {},
            { headers: { "Content-Type": "application/json" } }
        );
    },

    async getStatus(): Promise<ImportStatus> {
        const res = await importApi.api.get<ImportStatus>("/status");

        return res.data;
    },
};
