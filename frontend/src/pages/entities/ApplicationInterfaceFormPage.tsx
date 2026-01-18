import React, { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useController, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { apiClient } from '../../api/client';
import { ApplicationInterfaceFormSchema, ApplicationInterfaceFormData } from '../../utils/formValidation';
import { useEntity } from '../../hooks/useEntity';
import { EntityFormTemplate } from '../../components/forms/EntityFormTemplate';
import { FormFieldWrapper } from '../../components/forms/FormFieldWrapper';
import { DynamicFieldArray } from '../../components/forms/DynamicFieldArray';
import { DiscardChangesModal } from '../../components/forms/DiscardChangesModal';
import { hasUnsavedChanges, deepClone } from '../../utils/formHelpers';
import './EntityFormPage.css';

interface ApplicationInterfaceFormPageProps {
  isEdit?: boolean;
}

export function ApplicationInterfaceFormPage({ isEdit = false }: ApplicationInterfaceFormPageProps) {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [showDiscardModal, setShowDiscardModal] = useState(false);

  const { data: existingInterface, isLoading: isLoadingInterface } = useEntity(
    isEdit ? 'application-interfaces' : '',
    isEdit ? id : ''
  );

  const {
    register,
    control,
    watch,
    handleSubmit: handleReactFormSubmit,
    formState: { errors, isSubmitting },
    setError,
    reset,
  } = useForm<ApplicationInterfaceFormData>({
    resolver: zodResolver(ApplicationInterfaceFormSchema),
    defaultValues: isEdit
      ? existingInterface || {}
      : {
          name: '',
          description: '',
          application: '',
          type: 'REST',
          protocol: 'HTTPS',
          status: 'Active',
          owner: '',
          baseUrl: '',
          apiVersion: '',
          rateLimit: 1000,
          authenticationType: 'OAuth2',
          tags: [],
        },
  });

  const { field: tagsField } = useController({
    control,
    name: 'tags',
  });

  const watchedValues = watch();
  const [originalValues] = useState(() => deepClone(watchedValues));
  const hasChanges = hasUnsavedChanges(originalValues, watchedValues);

  useEffect(() => {
    if (existingInterface) {
      reset(existingInterface);
    }
  }, [existingInterface, reset]);

  const onSubmit = async (data: ApplicationInterfaceFormData) => {
    try {
      let response;

      if (isEdit && id) {
        response = await apiClient.patch(`/application-interfaces/${id}`, data);
      } else {
        response = await apiClient.post('/application-interfaces', data);
      }

      if (response.data?.id) {
        navigate(`/entities/application-interfaces/${response.data.id}`);
      }
    } catch (error: any) {
      const errorMessage =
        error?.response?.data?.detail || error?.message || 'Failed to save application interface';

      if (error?.response?.status === 422 && error?.response?.data?.errors) {
        error.response.data.errors.forEach((err: any) => {
          if (err.field) {
            setError(err.field as any, {
              type: 'server',
              message: err.message,
            });
          }
        });
      }
    }
  };

  const handleCancel = () => {
    if (hasChanges) {
      setShowDiscardModal(true);
    } else {
      navigate('/entities/application-interfaces');
    }
  };

  const handleConfirmDiscard = () => {
    setShowDiscardModal(false);
    navigate('/entities/application-interfaces');
  };

  if (isEdit && isLoadingInterface) {
    return <div className="entity-form-loading">Loading...</div>;
  }

  return (
    <>
      <EntityFormTemplate
        title={isEdit ? 'Edit Application Interface' : 'Create Application Interface'}
        subtitle={
          isEdit
            ? `Editing: ${existingInterface?.name || ''}`
            : 'Add a new application interface'
        }
        isEdit={isEdit}
        isLoading={isSubmitting}
        error={null}
        onSubmit={handleReactFormSubmit(onSubmit)}
        onCancel={handleCancel}
        submitButtonLabel={isEdit ? 'Save Changes' : 'Create Interface'}
      >
        <div className="entity-form-section">
          <h3 className="entity-form-section-title">Basic Information</h3>

          <div className="entity-form-section-grid">
            <FormFieldWrapper
              label="Interface Name"
              required
              error={errors.name?.message}
              htmlFor="name"
            >
              <input
                id="name"
                type="text"
                placeholder="e.g., Orders REST API"
                {...register('name')}
              />
            </FormFieldWrapper>

            <FormFieldWrapper
              label="Application"
              required
              error={errors.application?.message}
              htmlFor="application"
            >
              <input
                id="application"
                type="text"
                placeholder="Enter application ID"
                {...register('application')}
              />
            </FormFieldWrapper>

            <FormFieldWrapper
              label="Owner"
              required
              error={errors.owner?.message}
              htmlFor="owner"
            >
              <input
                id="owner"
                type="text"
                placeholder="Enter owner user ID"
                {...register('owner')}
              />
            </FormFieldWrapper>
          </div>

          <FormFieldWrapper
            label="Description"
            error={errors.description?.message}
            htmlFor="description"
          >
            <textarea
              id="description"
              placeholder="Describe the interface..."
              {...register('description')}
            />
          </FormFieldWrapper>
        </div>

        <div className="entity-form-section">
          <h3 className="entity-form-section-title">Interface Configuration</h3>

          <div className="entity-form-section-grid">
            <FormFieldWrapper
              label="Type"
              required
              error={errors.type?.message}
              htmlFor="type"
            >
              <select id="type" {...register('type')}>
                <option value="REST">REST</option>
                <option value="SOAP">SOAP</option>
                <option value="GraphQL">GraphQL</option>
                <option value="Message">Message Queue</option>
              </select>
            </FormFieldWrapper>

            <FormFieldWrapper
              label="Protocol"
              required
              error={errors.protocol?.message}
              htmlFor="protocol"
            >
              <select id="protocol" {...register('protocol')}>
                <option value="HTTP">HTTP</option>
                <option value="HTTPS">HTTPS</option>
                <option value="AMQP">AMQP</option>
                <option value="JMS">JMS</option>
                <option value="Kafka">Kafka</option>
                <option value="WebSocket">WebSocket</option>
              </select>
            </FormFieldWrapper>

            <FormFieldWrapper
              label="Status"
              required
              error={errors.status?.message}
              htmlFor="status"
            >
              <select id="status" {...register('status')}>
                <option value="Active">Active</option>
                <option value="Inactive">Inactive</option>
                <option value="Deprecated">Deprecated</option>
              </select>
            </FormFieldWrapper>

            <FormFieldWrapper
              label="Authentication Type"
              error={errors.authenticationType?.message}
              htmlFor="authenticationType"
            >
              <input
                id="authenticationType"
                type="text"
                placeholder="e.g., OAuth2, API Key"
                {...register('authenticationType')}
              />
            </FormFieldWrapper>
          </div>

          <div className="entity-form-section-grid">
            <FormFieldWrapper
              label="Base URL"
              error={errors.baseUrl?.message}
              htmlFor="baseUrl"
              helpText="Include https:// or http://"
            >
              <input
                id="baseUrl"
                type="text"
                placeholder="https://api.example.com"
                {...register('baseUrl')}
              />
            </FormFieldWrapper>

            <FormFieldWrapper
              label="API Version"
              error={errors.apiVersion?.message}
              htmlFor="apiVersion"
            >
              <input
                id="apiVersion"
                type="text"
                placeholder="e.g., v1, 2024-01"
                {...register('apiVersion')}
              />
            </FormFieldWrapper>

            <FormFieldWrapper
              label="Rate Limit"
              error={errors.rateLimit?.message}
              htmlFor="rateLimit"
              helpText="Requests per time unit"
            >
              <input
                id="rateLimit"
                type="number"
                min="0"
                placeholder="1000"
                {...register('rateLimit', { valueAsNumber: true })}
              />
            </FormFieldWrapper>
          </div>

          <DynamicFieldArray
            label="Tags"
            value={tagsField.value || []}
            onChange={tagsField.onChange}
            placeholder="e.g., public-api, core"
            error={errors.tags?.message}
          />
        </div>
      </EntityFormTemplate>

      <DiscardChangesModal
        isOpen={showDiscardModal}
        onConfirm={handleConfirmDiscard}
        onCancel={() => setShowDiscardModal(false)}
      />
    </>
  );
}
