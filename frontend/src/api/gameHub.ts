import {HubConnectionBuilder} from '@microsoft/signalr';

export const createGameConnection = (gameId: string, token: string) => {
    const connection = new HubConnectionBuilder()
        .withUrl(`${import.meta.env.VITE_SIGNALR_URL}?gameId=${gameId}`, {
            accessTokenFactory: () => token,
        })
        .withAutomaticReconnect()
        .build();

    return connection;
}