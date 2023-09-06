import api, { ImportSubjectDto } from "../../api";
import { promiseWithToast } from "../../utils";
import {
  Alert,
  AlertDescription,
  AlertIcon,
  Button,
  HStack,
  Icon,
  IconButton,
  Progress,
  Tooltip,
} from "@chakra-ui/react";
import React from "react";
import { IoLinkOutline, IoOpenOutline, IoRefresh, IoVideocamOutline } from "react-icons/io5";
import useSWRMutation from "swr/mutation";
import MIMEType from "whatwg-mimetype";

export const ItemLinks: React.FunctionComponent<{ id: number }> = ({ id }) => {
  const { data: links, error } = api.useItemLinkList(id);
  const bangumi =
    links &&
    links.find((x) => x.address.startsWith("https://bgm.tv/subject/") && x.mime_type === "text/html;kind=bgm.tv");
  const onRefresh = () =>
    promiseWithToast(async () => {
      if (!bangumi) {
        return;
      }
      await trigger({
        id: Number.parseInt(bangumi.address.replace("https://bgm.tv/subject/", ""), 10),
      });
    });
  // FIXME: extract this pattern to api generation
  const { trigger, isMutating } = useSWRMutation(
    "/api/modules/bangumi/import_subject",
    (_url, { arg }: { arg: ImportSubjectDto }) => api.bangumiImportSubject(arg),
    {},
  );

  return (
    <>
      {error && (
        <Alert status="error">
          <AlertIcon />
          <AlertDescription>{error.message}</AlertDescription>
        </Alert>
      )}
      {!links && <Progress width="100%" size="xs" colorScheme="teal" isIndeterminate />}
      <HStack spacing={1}>
        {links &&
          links.map((i) => {
            const commonProps = {
              "aria-label": i.address,
              size: "xs",
              as: "a" as const,
              href: i.address,
              target: "_blank",
            };
            let body: React.ReactElement;
            const mimeType = MIMEType.parse(i.mime_type);
            if (i.mime_type === "text/html;kind=bgm.tv") {
              body = (
                <Button leftIcon={<Icon as={IoOpenOutline} />} {...commonProps}>
                  bgm.tv
                </Button>
              );
            } else if (mimeType?.type === "video" || mimeType?.parameters.get("kind") === "video") {
              body = <IconButton icon={<Icon as={IoVideocamOutline} />} colorScheme="teal" {...commonProps} />;
            } else {
              body = <IconButton icon={<Icon as={IoLinkOutline} />} colorScheme="teal" {...commonProps} />;
            }
            return (
              <Tooltip label={i.address} key={i.id}>
                {body}
              </Tooltip>
            );
          })}
        {bangumi && (
          <Tooltip label="Refresh bgm.tv Data">
            <IconButton
              icon={<Icon as={IoRefresh} />}
              onClick={onRefresh}
              isLoading={isMutating}
              size="xs"
              aria-label="Refresh bgm.tv"
            />
          </Tooltip>
        )}
      </HStack>
    </>
  );
};
